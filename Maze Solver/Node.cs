using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maze_Solver
{
    class Node
    {
        public List<Node> connections = new List<Node>();
        public bool isStart { get; set; }
        public bool isEnd { get; set; }
        public bool visited { get; set; }
        public int xCoord { get; set; }
        public int yCoord { get; set; }

        public void connectTo(Node node)
        {
            connections.Add(node);
        }
    }
}
