using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTable
{
    public abstract class Equation
    {
        #region Public Methods

        abstract public bool Test(Dictionary<string, bool> keys);

        #endregion Public Methods
    }
}