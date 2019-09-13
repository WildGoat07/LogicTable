using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTable
{
    public class OperatorEquation : Equation
    {
        #region Public Enums

        public enum OperatorType
        {
            AND,
            OR,
            EQUIVALENT,
            INVOLVING
        }

        #endregion Public Enums

        #region Public Properties

        public Equation Left { get; set; }
        public OperatorType Operator { get; set; }
        public Equation Right { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override bool Test(Dictionary<string, bool> keys)
        {
            var left = Left.Test(keys);
            var right = Right.Test(keys);
            bool res;
            switch (Operator)
            {
                case OperatorType.AND:
                    res = left && right;
                    break;

                case OperatorType.OR:
                    res = left || right;
                    break;

                case OperatorType.EQUIVALENT:
                    res = Left.Test(keys) == Right.Test(keys);
                    break;

                case OperatorType.INVOLVING:
                    res = !Left.Test(keys) || Right.Test(keys);
                    break;

                default:
                    res = false;
                    break;
            }
            return res;
        }

        #endregion Public Methods
    }
}