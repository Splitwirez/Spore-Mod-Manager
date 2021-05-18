#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2014-2019 Martin Owens
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
Merges styles into class based styles and removes.
"""

import inkex

class MergeStyles(inkex.EffectExtension):
    """Merge any styles which are the same for CSS"""
    def add_arguments(self, pars):
        self.arg_parser.add_argument("-n", "--name", type=str, dest="name",\
             help="Name of selected element's common class")

    def effect(self):
        """Apply the style effect"""
        newclass = self.options.name
        if not newclass:
            newclass = self.svg.get_unique_id('css')

        elements = self.svg.selected.values()
        common = None

        for elem in elements:
            style = set(elem.style.items())
            if common is not None:
                common &= style
            else:
                common = style

        if not common:
            return inkex.errormsg("There are no common styles between these elements.")

        self.svg.stylesheet.add('.' + newclass, inkex.Style(sorted(common)))

        for elem in elements:
            elem.style -= dict(common).keys()
            elem.classes.append(newclass)
        return True

if __name__ == '__main__':
    MergeStyles().run()
