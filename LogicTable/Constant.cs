using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTable
{
    public class Constant : Equation
    {
        #region Public Properties

        public string Name { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override bool Test(Dictionary<string, bool> keys) => keys[Name];

        #endregion Public Methods
    }
}