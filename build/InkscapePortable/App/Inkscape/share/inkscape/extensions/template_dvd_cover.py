#!/usr/bin/env python
# -*- coding: utf-8 -*-
#
# Copyright (C) 2010 Tavmjong Bah
#               2019 Martin Owens <doctormo@gmail.com>
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
Generic template functionality controlled by the INX file.
"""

import inkex

class DvdCover(inkex.TemplateExtension):
    """Create an empty DVD Cover (in mm)"""
    multi_inx = True
    def add_arguments(self, pars):
        pars.add_argument("-s", "--spine", type=float, default=14.0,
                          help="Dvd spine width (mm)")
        pars.add_argument("-b", "--bleed", type=float, default=3.0,
                          help="Bleed (extra area around image")

    def get_size(self):
        # Dimensions in mm
        width = 259.0  # Before adding spine width or bleed
        height = 183.0  # Before adding bleed
        bleed = self.options.bleed
        spine = self.options.spine

        return (width + spine + bleed * 2.0, 'mm',
                height + bleed * 2.0, 'mm')

    def set_namedview(self, width, height, unit):
        super(DvdCover, self).set_namedview(width, height, unit)
        (width, _, height, unit) = self.get_size()
        bleed, spine = self.options.bleed, self.options.spine
        self.svg.namedview.new_guide(bleed, True, "bottom")
        self.svg.namedview.new_guide(height - bleed, True, "top")
        self.svg.namedview.new_guide(bleed, False, "left edge")
        self.svg.namedview.new_guide((width - spine) / 2.0, False, "left spline")
        self.svg.namedview.new_guide((width + spine) / 2.0, False, "right spline")
        self.svg.namedview.new_guide(width - bleed, False, "top")

if __name__ == '__main__':
    DvdCover().run()
