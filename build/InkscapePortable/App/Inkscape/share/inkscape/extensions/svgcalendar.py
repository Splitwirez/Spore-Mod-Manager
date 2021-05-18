#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2008 Aurelio A. Heckert <aurium(a)gmail.com>
#    Week number option added by Olav Vitters and Nicolas Dufour (2012)
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#
"""
A calendar generator plugin for Inkscape, but also can be used as a standalone
command line application.
#
More on ISO week number calculation on:
http://en.wikipedia.org/wiki/ISO_week_date
(The first week of a year is the week that contains the first Thursdayof the year.)
"""

__version__ = "0.3"

import datetime
import calendar
import re
import sys

import inkex
from inkex import TextElement

if sys.version_info[0] > 2:
    def unicode(s, encoding):
        if isinstance(s, bytes):
            return s.decode(encoding)
        return s


class Calendar(inkex.EffectExtension):
    """Generate Calendar in SVG"""
    def add_arguments(self, pars):
        pars.add_argument("--tab", type=str, dest="tab")
        pars.add_argument("--month", type=int, default=0,\
            help="Month to be generated. If 0, then the entry year will be generated.")
        pars.add_argument("--year", type=int, default=0,\
            help="Year to be generated. If 0, then the current year will be generated.")
        pars.add_argument("--fill-empty-day-boxes", type=inkex.Boolean,\
            dest="fill_edb", default=True, help="Fill empty day boxes with next month days.")
        pars.add_argument("--show-week-number", type=inkex.Boolean,\
            dest="show_weeknr", default=False, help="Include a week number column.")
        pars.add_argument("--start-day", default="sun", help='Week start day. ("sun" or "mon")')
        pars.add_argument("--weekend", default="sat+sun",\
            help='Define the weekend days. ("sat+sun" or "sat" or "sun")')
        pars.add_argument(
            "--auto-organize", type=inkex.Boolean, dest="auto_organize", default=True,
            help='Automatically set the size and positions.')
        pars.add_argument(
            "--months-per-line", type=int, dest="months_per_line", default=3,
            help='Number of months side by side.')
        pars.add_argument(
            "--month-width", type=str, dest="month_width", default="6cm",
            help='The width of the month days box.')
        pars.add_argument(
            "--month-margin", type=str, dest="month_margin", default="1cm",
            help='The space between the month boxes.')
        pars.add_argument(
            "--color-year", type=str, dest="color_year", default="#888",
            help='Color for the year header.')
        pars.add_argument(
            "--color-month", type=str, dest="color_month", default="#666",
            help='Color for the month name header.')
        pars.add_argument(
            "--color-day-name", type=str, dest="color_day_name", default="#999",
            help='Color for the week day names header.')
        pars.add_argument(
            "--color-day", type=str, dest="color_day", default="#000",
            help='Color for the common day box.')
        pars.add_argument(
            "--color-weekend", type=str, dest="color_weekend", default="#777",
            help='Color for the weekend days.')
        pars.add_argument(
            "--color-nmd", type=str, dest="color_nmd", default="#BBB",
            help='Color for the next month day, in empty day boxes.')
        pars.add_argument(
            "--color-weeknr", type=str, dest="color_weeknr", default="#808080",
            help='Color for the week numbers.')
        pars.add_argument(
            "--font-year", type=str, dest="font_year", default="arial",
            help='Font for the year string.')
        pars.add_argument(
            "--font-month", type=str, dest="font_month", default="arial",
            help='Font for the month strings.')
        pars.add_argument(
            "--font-day-name", type=str, dest="font_day_name", default="arial",
            help='Font for the days of the week strings.')
        pars.add_argument(
            "--font-day", type=str, dest="font_day", default="arial",
            help='Font for the day strings.')
        pars.add_argument(
            "--month-names", type=str, dest="month_names",
            default='January February March '
                    'April May June July '
                    'August September October '
                    'November December',
            help='The month names for localization.')
        pars.add_argument(
            "--day-names", type=str, dest="day_names", default='Sun Mon Tue Wed Thu Fri Sat',
            help='The week day names for localization.')
        pars.add_argument(
            "--weeknr-name", type=str, dest="weeknr_name", default='Wk',
            help='The week number column name for localization.')
        pars.add_argument(
            "--encoding", type=str, dest="input_encode", default='utf-8',
            help='The input encoding of the names.')

    def validate_options(self):
        # inkex.errormsg( self.options.input_encode )
        # Convert string names lists in real lists
        m = re.match(r'\s*(.*[^\s])\s*', self.options.month_names)
        self.options.month_names = re.split(r'\s+', m.group(1))
        m = re.match(r'\s*(.*[^\s])\s*', self.options.day_names)
        self.options.day_names = re.split(r'\s+', m.group(1))
        # Validate names lists
        if len(self.options.month_names) != 12:
            inkex.errormsg('The month name list "' +
                           str(self.options.month_names) +
                           '" is invalid. Using default.')
            self.options.month_names = ['January', 'February', 'March',
                                        'April', 'May', 'June',
                                        'July', 'August', 'September',
                                        'October', 'November', 'December']
        if len(self.options.day_names) != 7:
            inkex.errormsg('The day name list "' +
                           str(self.options.day_names) +
                           '" is invalid. Using default.')
            self.options.day_names = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu',
                                      'Fri', 'Sat']
        # Convert year 0 to current year
        if self.options.year == 0:
            self.options.year = datetime.datetime.today().year
        # Year 1 starts it's week at monday, obligatorily
        if self.options.year == 1:
            self.options.start_day = 'mon'
        # Set the calendar start day
        if self.options.start_day == 'sun':
            calendar.setfirstweekday(6)
        else:
            calendar.setfirstweekday(0)
        # Convert string numbers with unit to user space float numbers
        self.options.month_width = self.svg.unittouu(self.options.month_width)
        self.options.month_margin = self.svg.unittouu(self.options.month_margin)

    # initial values
    month_x_pos = 0
    month_y_pos = 0
    weeknr = 0

    def calculate_size_and_positions(self):
        # month_margin month_width months_per_line auto_organize
        self.doc_w = self.svg.unittouu(self.document.getroot().get('width'))
        self.doc_h = self.svg.unittouu(self.document.getroot().get('height'))
        if self.options.show_weeknr:
            self.cols_before = 1
        else:
            self.cols_before = 0
        if self.options.auto_organize:
            if self.doc_h > self.doc_w:
                self.months_per_line = 3
            else:
                self.months_per_line = 4
        else:
            self.months_per_line = self.options.months_per_line
        # self.month_w = self.doc_w / self.months_per_line
        if self.options.auto_organize:
            self.month_w = (self.doc_w * 0.8) / self.months_per_line
            self.month_margin = self.month_w / 10
        else:
            self.month_w = self.options.month_width
            self.month_margin = self.options.month_margin
        self.day_w = self.month_w / (7 + self.cols_before)
        self.day_h = self.month_w / 9
        self.month_h = self.day_w * 7
        if self.options.month == 0:
            self.year_margin = ((self.doc_w + self.day_w -
                                 (self.month_w * self.months_per_line) -
                                 (self.month_margin *
                                  (self.months_per_line - 1))) / 2)  # - self.month_margin
        else:
            self.year_margin = (self.doc_w - self.month_w) / 2
        self.style_day = {
            'font-size': str(self.day_w / 2),
            'font-family': self.options.font_day,
            'text-anchor': 'middle',
            'text-align': 'center',
            'fill': self.options.color_day
        }
        self.style_weekend = self.style_day.copy()
        self.style_weekend['fill'] = self.options.color_weekend
        self.style_nmd = self.style_day.copy()
        self.style_nmd['fill'] = self.options.color_nmd
        self.style_month = self.style_day.copy()
        self.style_month['fill'] = self.options.color_month
        self.style_month['font-family'] = self.options.font_month
        self.style_month['font-size'] = str(self.day_w / 1.5)
        self.style_month['font-weight'] = 'bold'
        self.style_day_name = self.style_day.copy()
        self.style_day_name['fill'] = self.options.color_day_name
        self.style_day_name['font-family'] = self.options.font_day_name
        self.style_day_name['font-size'] = str(self.day_w / 3)
        self.style_year = self.style_day.copy()
        self.style_year['fill'] = self.options.color_year
        self.style_year['font-family'] = self.options.font_year
        self.style_year['font-size'] = str(self.day_w * 2)
        self.style_year['font-weight'] = 'bold'
        self.style_weeknr = self.style_day.copy()
        self.style_weeknr['fill'] = self.options.color_weeknr
        self.style_weeknr['font-size'] = str(self.day_w / 3)

    def is_weekend(self, pos):
        # weekend values: "sat+sun" or "sat" or "sun"
        if self.options.start_day == 'sun':
            if self.options.weekend == 'sat+sun' and pos == 0:
                return True
            if self.options.weekend == 'sat+sun' and pos == 6:
                return True
            if self.options.weekend == 'sat' and pos == 6:
                return True
            if self.options.weekend == 'sun' and pos == 0:
                return True
        else:
            if self.options.weekend == 'sat+sun' and pos == 5:
                return True
            if self.options.weekend == 'sat+sun' and pos == 6:
                return True
            if self.options.weekend == 'sat' and pos == 5:
                return True
            if self.options.weekend == 'sun' and pos == 6:
                return True
        return False

    def in_line_month(self, cal):
        cal2 = []
        for week in cal:
            for day in week:
                if day != 0:
                    cal2.append(day)
        return cal2

    def write_month_header(self, g, m):
        txt_atts = {'style': str(inkex.Style(self.style_month)),
                    'x': str((self.month_w - self.day_w) / 2),
                    'y': str(self.day_h / 5)}
        try:
            g.add(TextElement(**txt_atts)).text = unicode(
                self.options.month_names[m - 1],
                self.options.input_encode)
        except:
            raise ValueError('You must select a correct system encoding.')

        gw = g.add(inkex.Group())
        week_x = 0
        if self.options.start_day == 'sun':
            day_names = self.options.day_names[:]
        else:
            day_names = self.options.day_names[1:]
            day_names.append(self.options.day_names[0])

        if self.options.show_weeknr:
            day_names.insert(0, self.options.weeknr_name)

        for wday in day_names:
            txt_atts = {'style': str(inkex.Style(self.style_day_name)),
                        'x': str(self.day_w * week_x),
                        'y': str(self.day_h)}
            try:
                gw.add(TextElement(**txt_atts)).text = unicode(
                    wday, self.options.input_encode)
            except:
                raise ValueError('You must select a correct system encoding.')

            week_x += 1

    def create_month(self, m):
        txt_atts = {
            'transform': 'translate(' +
                         str(self.year_margin +
                             (self.month_w + self.month_margin) *
                             self.month_x_pos) +
                         ',' +
                         str((self.day_h * 4) +
                             (self.month_h * self.month_y_pos)) +
                         ')',
            'id': 'month_' +
                  str(m) +
                  '_' +
                  str(self.options.year)}
        g = self.year_g.add(inkex.Group(**txt_atts))
        self.write_month_header(g, m)
        gdays = g.add(inkex.Group())
        cal = calendar.monthcalendar(self.options.year, m)
        if m == 1:
            if self.options.year > 1:
                before_month = \
                    self.in_line_month(calendar.monthcalendar(self.options.year - 1, 12))
        else:
            before_month = \
                self.in_line_month(calendar.monthcalendar(self.options.year, m - 1))
        if m == 12:
            next_month = \
                self.in_line_month(calendar.monthcalendar(self.options.year + 1, 1))
        else:
            next_month = \
                self.in_line_month(calendar.monthcalendar(self.options.year, m + 1))
        if len(cal) < 6:
            # add a line after the last week
            cal.append([0, 0, 0, 0, 0, 0, 0])
        if len(cal) < 6:
            # add a line before the first week (Feb 2009)
            cal.reverse()
            cal.append([0, 0, 0, 0, 0, 0, 0])
            cal.reverse()
        # How mutch before month days will be showed:
        bmd = cal[0].count(0) + cal[1].count(0)
        before = True
        week_y = 0
        for week in cal:
            if (self.weeknr != 0 and
                ((self.options.start_day == 'mon' and week[0] != 0) or
                 (self.options.start_day == 'sun' and week[1] != 0))) or \
                    (self.weeknr == 0 and
                     ((self.options.start_day == 'mon' and week[3] > 0) or
                      (self.options.start_day == 'sun' and week[4] > 0))):
                self.weeknr += 1
            week_x = 0
            if self.options.show_weeknr:
                # Remove leap week (starting previous year) and empty weeks
                if self.weeknr != 0 and not (week[0] == 0 and week[6] == 0):
                    style = self.style_weeknr
                    txt_atts = {'style': str(inkex.Style(style)),
                                'x': str(self.day_w * week_x),
                                'y': str(self.day_h * (week_y + 2))}
                    gdays.add(TextElement(**txt_atts)).text = str(self.weeknr)
                    week_x += 1
                else:
                    week_x += 1
            for day in week:
                style = self.style_day
                if self.is_weekend(week_x - self.cols_before):
                    style = self.style_weekend
                if day == 0:
                    style = self.style_nmd
                txt_atts = {'style': str(inkex.Style(style)),
                            'x': str(self.day_w * week_x),
                            'y': str(self.day_h * (week_y + 2))}
                text = None
                if day == 0 and not self.options.fill_edb:
                    pass  # draw nothing
                elif day == 0:
                    if before:
                        text = str(before_month[-bmd])
                        bmd -= 1
                    else:
                        text = str(next_month[bmd])
                        bmd += 1
                else:
                    text = str(day)
                    before = False
                if text:
                    gdays.add(TextElement(**txt_atts)).text = text
                week_x += 1
            week_y += 1
        self.month_x_pos += 1
        if self.month_x_pos >= self.months_per_line:
            self.month_x_pos = 0
            self.month_y_pos += 1

    def effect(self):
        self.validate_options()
        self.calculate_size_and_positions()
        parent = self.document.getroot()
        txt_atts = {'id': 'year_' + str(self.options.year)}
        self.year_g = parent.add(inkex.Group(**txt_atts))
        txt_atts = {'style': str(inkex.Style(self.style_year)),
                    'x': str(self.doc_w / 2),
                    'y': str(self.day_w * 1.5)}
        self.year_g.add(TextElement(**txt_atts)).text = str(self.options.year)
        try:
            if self.options.month == 0:
                for m in range(1, 13):
                    self.create_month(m)
            else:
                self.create_month(self.options.month)
        except ValueError as err:
            return inkex.errormsg(str(err))

if __name__ == '__main__':
    Calendar().run()
