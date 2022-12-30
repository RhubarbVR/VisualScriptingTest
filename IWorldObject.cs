using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript
{
    public interface IWorldObject : IDisposable
    {
		public int ID { get; }
		public bool Enabled { get; set; }
        public App App { get; }
		internal void Step();

		internal void Init(App app);
        public string Name { get; }
    }
}
