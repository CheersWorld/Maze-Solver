using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maze_Solver
{
    class Connection
    {
    
        private List<Node> nodes = new List<Node>();

        public Connection(Node node1, Node node2)
        {
            nodes.Add(node1);
            nodes.Add(node2);
        }
    }
}
