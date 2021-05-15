#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2007 Martin Owens
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
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110, USA.
#
"""
Inkscape's general barcode extension. Run from within inkscape or use the
Barcode module provided for outside or scripting.
"""

import inkex
from barcode import get_barcode

class Barcode(inkex.GenerateExtension):
    """
    Raw barcode Effect class, see Barcode base class.
    """
    def add_arguments(self, pars):
        pars.add_argument("-l", "--height", type=int, default=30, help="Barcode Height")
        pars.add_argument("-t", "--type", default='', help="Barcode Type")
        pars.add_argument("-d", "--text", default='', help="Text to print on barcode")

    def generate(self):
        (pos_x, pos_y) = self.svg.namedview.center

        return get_barcode(
            self.options.type,
            text=self.options.text,
            height=self.options.height,
            document=self.document,
            x=pos_x, y=pos_y,
            scale=self.svg.unittouu('1px'),
        ).generate()

if __name__ == '__main__':
    Barcode().run()
