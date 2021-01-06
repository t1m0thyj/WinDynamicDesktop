// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace WinDynamicDesktop
{
    // Code based on https://mikhail.io/2016/01/validation-with-either-data-type-in-csharp/
    public class ThemeResult
    {
        private readonly ThemeError left;
        private readonly ThemeConfig right;
        private readonly bool isLeft;

        public ThemeResult(ThemeError left)
        {
            this.left = left;
            this.isLeft = true;
        }

        public ThemeResult(ThemeConfig right)
        {
            this.right = right;
            this.isLeft = false;
        }

        public T Match<T>(Func<ThemeError, T> leftFunc, Func<ThemeConfig, T> rightFunc)
        {
            if (leftFunc == null)
            {
                throw new ArgumentNullException(nameof(leftFunc));
            }

            if (rightFunc == null)
            {
                throw new ArgumentNullException(nameof(rightFunc));
            }

            return this.isLeft ? leftFunc(this.left) : rightFunc(this.right);
        }

        public void Match(Action<ThemeError> leftAction, Action<ThemeConfig> rightAction)
        {
            if (leftAction == null)
            {
                throw new ArgumentNullException(nameof(leftAction));
            }

            if (rightAction == null)
            {
                throw new ArgumentNullException(nameof(rightAction));
            }

            if (this.isLeft)
            {
                leftAction(this.left);
            }
            else
            {
                rightAction(this.right);
            }
        }
    }
}
