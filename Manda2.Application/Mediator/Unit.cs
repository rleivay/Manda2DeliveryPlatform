using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Mediator
{
    public sealed class Unit
    {
        public static readonly Unit Value = new();
        private Unit() { }
    }
}
