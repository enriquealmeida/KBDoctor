using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concepto.Packages.KBDoctorCore.Sources
{
    class KBDAST
    {
        public int id;
        public int command;
        public List<int> tokens;
        private KBDAST inside;
        private KBDAST next;
        private KBDAST parent;
        public int cant_nodes_in;
        public int cant_nodes_next;
        public KBDAST(int id, int command, List<int> tokens)
        {
            this.id = id;
            this.command = command;
            this.tokens = new List<int>(tokens);
            cant_nodes_in = 0;
            cant_nodes_next = 0;
            inside = null;
            next = null;
        }
        public KBDAST GetNextSubTree()
        {
            return next;
        }
        public KBDAST GetInsideSubTree()
        {
            return inside;
        }
        public void SetNextSubTree(KBDAST next)
        {
            this.next = next;
            this.cant_nodes_next = next.cant_nodes_in + next.cant_nodes_next + 1;
            this.next.parent = this;
        }
        public void SetInsideSubTree(KBDAST inside)
        {
            this.inside = inside;
            this.cant_nodes_in = inside.cant_nodes_next + inside.cant_nodes_in + 1;
            this.inside.parent = this;
        }
        public int SubTreeCardinality()
        {
            return this.cant_nodes_next + this.cant_nodes_in + 1;
        }
    }
}
