using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norion.Calendar;

public interface ICalendar
{
    bool IsPublicHoliday(DateTime date);
}
