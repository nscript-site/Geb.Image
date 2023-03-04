using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geb.Image.Skia
{
    public class SkException:Exception
    {
        public SkException(String? msg = null):base(msg)
        {
        }
    }
}
