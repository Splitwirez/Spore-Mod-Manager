#!/usr/bin/env python
#
# Copyright 2008, 2009 Hannes Hochreiner
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see http://www.gnu.org/licenses/.
#
"""Automatic text for jessyInk"""


import inkex
from jessyink_install import JessyInkMixin, _

class AutoTexts(JessyInkMixin, inkex.EffectExtension):
    """Add AutoText to jessyInk"""
    def add_arguments(self, pars):
        pars.add_argument('--tab', dest='what')
        pars.add_argument('--autoText', default='none')

    def effect(self):
        self.is_installed()

        if not self.svg.selected:
            inkex.errormsg(_("To assign an effect, please select an object.\n\n"))

        for node in self.svg.selected.get(inkex.Tspan).values():
            if self.options.autoText == "slideTitle":
                node.set("jessyink:autoText", "slideTitle")
            elif self.options.autoText == "slideNumber":
                node.set("jessyink:autoText", "slideNumber")
            elif self.options.autoText == "numberOfSlides":
                node.set("jessyink:autoText", "numberOfSlides")
            elif node.get("jessyink:autoText"):
                node.set("jessyink:autoText", None)

if __name__ == '__main__':
    AutoTexts().run()
