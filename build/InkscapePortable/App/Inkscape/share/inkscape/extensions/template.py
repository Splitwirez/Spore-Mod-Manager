#!/usr/bin/env python
# -*- coding: utf-8 -*-
#
# Copyright (C) 2018 Martin Owens <doctormo@gmail.com>
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

class InxDefinedTemplate(inkex.TemplateExtension):
    """Most functionality is in TemplateExtension"""
    multi_inx = True
    themes = {
        'white': ('#ffffff', '#666666'),
        'gray': ('#808080', '#444444'),
        'black': ('#000000', '#999999'),
    }

    def add_arguments(self, pars):
        pars.add_argument("--background", type=self.arg_theme, default="normal")
        pars.add_argument("--noborder", type=inkex.Boolean)

    def arg_theme(self, value):
        """Set the page theme based on the value given"""
        if value in self.themes:
            return self.themes[value]
        return None

    def set_namedview(self, width, height, unit):
        super(InxDefinedTemplate, self).set_namedview(width, height, unit)
        namedview = self.svg.namedview
        if self.options.background:
            namedview.set('pagecolor', self.options.background[0])
            namedview.set('bordercolor', self.options.background[1])
            namedview.set('inkscape:pageopacity', "1.0")
            namedview.set('inkscape:pageshadow', "0")

        if self.options.noborder:
            namedview.set('bordercolor', namedview.get('pagecolor'))
            namedview.set('borderopacity', "0")

if __name__ == '__main__':
    InxDefinedTemplate().run()
