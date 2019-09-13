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
            switch (Operator)
            {
                case OperatorType.AND:
                    return Left.Test(keys) && Right.Test(keys);

                case OperatorType.OR:
                    return Left.Test(keys) || Right.Test(keys);

                case OperatorType.EQUIVALENT:
                    return Left.Test(keys) == Right.Test(keys);

                case OperatorType.INVOLVING:
                    return !Left.Test(keys) || Right.Test(keys);

                default:
                    return false;
            }
        }

        #endregion Public Methods
    }
}