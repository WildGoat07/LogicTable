using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTable
{
    public class Not : Equation
    {
        #region Public Properties

        public Equation InternalEquation { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override bool Test(Dictionary<string, bool> keys) => !InternalEquation.Test(keys);

        #endregion Public Methods
    }
}