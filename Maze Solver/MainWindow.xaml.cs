using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;

namespace Maze_Solver {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window {
        public MainWindow() {

            InitializeComponent();
        }

        Field[,] mazeArray = new Field[0, 0];
        int imageWidth;
        int imageHeigth;
        String FilePath;

        //Choosing the Maze
        private void File_Chooser_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "png files (*.png)|*.png";
            ofd.InitialDirectory = "D:\\C#\\Maze Solver\\Mazes";

            if (ofd.ShowDialog() == true) {
                FilePath = ofd.FileName;
                FilePathTextBox.Text = ofd.FileName;
            }

        }

        //Importin the maze
        private void ImportButton_Click(object sender, RoutedEventArgs e) {
            addTextToLog("Importing image");
            try {
                Bitmap mazeImage = new Bitmap(FilePath);
                imageHeigth = mazeImage.Height;
                imageWidth = mazeImage.Width;
                mazeArray = new Field[imageHeigth, imageWidth];
                for (int y = 0; y < mazeImage.Height; y++) {
                    for (int x = 0; x < mazeImage.Width; x++) {
                        Color pixelColor = mazeImage.GetPixel(x, y);
                        string pixelColorStringValue =
                            pixelColor.R.ToString("D3") + " " +
                            pixelColor.G.ToString("D3") + " " +
                            pixelColor.B.ToString("D3") + ", ";

                        Field currentField = new Field();
                        currentField.xCoord = x;
                        currentField.yCoord = y;


                        switch (pixelColorStringValue) { //Assigning Paths and Walls according to Pixel color
                            case "255 255 255, ":
                                currentField.isPath = true;
                                break;

                            case "000 000 000, ":
                                currentField.isPath = false;
                                break;

                        }
                        mazeArray[y, x] = currentField;
                    }
                }

            }
            catch (Exception err) {
                MessageBox.Show("An error occured: " + err, "Error");
            }
            addTextToLog("Image successfully imported");
            Node[,] nodesArray = GetNodes();
            addTextToLog("Finished node search, starting solving");
            solveMaze(nodesArray);

        }

        //Places Nodes at every junction to reduce the amount of steps needed to solve the maze
        private Node[,] GetNodes() {
            Field previousField = new Field();
            Node previousNode = new Node();
            previousField.yCoord = -1;
            List<Node> nodes = new List<Node>();
            Node[,] nodesArray = new Node[imageHeigth, imageWidth];
            foreach (Field field in mazeArray) {
                if (IsJunction(field) || ((field.yCoord == 0 || field.yCoord == imageHeigth - 1) && field.isPath)) { //Places a node if the field is a junction
                    Node node = new Node();
                    node.xCoord = field.xCoord;
                    node.yCoord = field.yCoord;
                    if (field.yCoord == 0) {
                        node.isStart = true;
                    }
                    if (field.yCoord == imageHeigth - 1) {
                        node.isEnd = true;
                    }
                    if (isWallBetween(node, previousNode)) { //Connects nodes, if there is no wall between them 
                        node.connectTo(previousNode);
                        previousNode.connectTo(node);
                    }
                    nodesArray[field.yCoord, field.xCoord] = node;
                    previousNode = node;
                    previousField = field;
                }
            }

            //Connecting the nodes, upwards this time. 
            foreach (Node node in nodesArray) {
                if (node == null) {
                    continue;
                }
                int i = 1;
                while (i <= node.yCoord && mazeArray[node.yCoord - i, node.xCoord].isPath) {
                    if (nodesArray[node.yCoord - i, node.xCoord] == null) {
                        //Just a regular path, well ignore that
                    }
                    else {
                        node.connectTo(nodesArray[node.yCoord - i, node.xCoord]);
                        nodesArray[node.yCoord - i, node.xCoord].connectTo(node);
                    }
                    i++;
                }

            }

            foreach (Node node in nodesArray) {
                if (node == null) {
                    continue;
                }
                nodesArray = removeDeadEnds(node, nodesArray);
                if (node != null) {
                    nodes.Add(node);
                }
            }
            addTextToLog(nodes.Count + " Nodes found (purged dead ends)");
            return nodesArray;
        }

        private bool IsJunction(Field field) {
            if (field.isPath == false) {
                return false;
            }
            bool pathLeft = false;
            bool pathRight = false;
            bool pathUp = false;
            bool pathDown = false;
            Field[,] maze = mazeArray;
            try {
                if (field.xCoord >= 1) {
                    pathLeft = maze[field.yCoord, field.xCoord - 1].isPath;
                }
                if (field.xCoord <= imageWidth - 2) {
                    pathRight = maze[field.yCoord, field.xCoord + 1].isPath;
                }
                if (field.yCoord > 0) {
                    pathUp = maze[field.yCoord - 1, field.xCoord].isPath;
                }
                if (field.yCoord <= imageHeigth - 2) {
                    pathDown = maze[field.yCoord + 1, field.xCoord].isPath;
                }
            }
            catch (Exception error) {
                MessageBox.Show("A handled Exception occured during junction finding: " + error);
            }
            return ((pathUp || pathDown) && (pathLeft || pathRight));
        }

        private void addTextToLog(String text) {
            TextBox logBox = LogView;
            String temp_text = logBox.Text;
            temp_text = temp_text + text + "\r\n";
            logBox.Text = temp_text;
        }

        private bool isWallBetween(Node newNode, Node previousNode) {
            int difference = newNode.xCoord - previousNode.xCoord;
            for (int i = 1; i < difference; i++) {
                if (!mazeArray[newNode.yCoord, newNode.xCoord - i].isPath) {
                    return false;
                }
            }
            return true;
        }

        private Node[,] removeDeadEnds(Node node, Node[,] nodesArray) {
            if (node.connections.Count == 1 && node.isStart == false && node.isEnd == false) {
            ConnectionIteration:
                int index = 0;
                foreach (Node connectedNode in node.connections[0].connections) {
                    //Removing the current node from every other nodes connection list
                    if ((connectedNode.xCoord == node.xCoord && connectedNode.yCoord == node.yCoord) || nodesArray[connectedNode.yCoord, connectedNode.xCoord] == null) {
                        //Removing all nodes from the connection list where either the node array is zero or the cooridnates match
                        node.connections[0].connections.RemoveAt(index);
                        goto ConnectionIteration;
                    }
                    index++;
                }
                nodesArray[node.yCoord, node.xCoord] = null;
                nodesArray = removeDeadEnds(node.connections[0], nodesArray);
            }
            return nodesArray;
        }

        private void solveMaze(Node[,] nodesArray) {
            Node startNode = new Node();
            foreach (Node node in nodesArray) {
                if (node == null) {
                    continue;
                }
                if (node.isStart) {
                    startNode = node;
                    break;
                }
            }
            List<Node> path = new List<Node>();
            path = followConnections(startNode, path);
            addTextToLog("Finished solving, steps needed: " + path.Count);
            addTextToLog("Path to exit: ");
            foreach (Node node in path) {
                addTextToLog("Node: " + node.yCoord + " " + node.xCoord);
            }
        }

        /*private List<Node> followConnections(Node node, List<Node> path) {
            Node connectedNode = node.connections[0];
                while(connectedNode.isEnd == false) {
                    if (connectedNode.visited) {
                        continue;
                    }
                    if (connectedNode.isEnd) {
                        break;
                    }
                    else {
                        connectedNode.visited = true;
                        path.Add(connectedNode);
                        connectedNode = connectedNode.connections[0];
                        if (connectedNode.visited) {
                            connectedNode = connectedNode.connections[1];
                        }
                    }
            }
            return path;
        }*/

        private List<Node> followConnections(Node node, List<Node> path) {
            foreach (Node connectedNode in node.connections) {
                if (connectedNode.visited) {
                    continue;
                }
                if (connectedNode.isEnd) {
                    //Stops the node search by skipping the next step
                }
                else {
                    connectedNode.visited = true;
                    path.Add(connectedNode);
                    path = followConnections(connectedNode, path);
                }
            }
            return path;
        }
    }
}