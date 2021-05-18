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
"""Add mouse handler for jessyInk"""

import inkex
from inkex.elements import BaseElement, Script
from jessyink_install import JessyInkMixin

class MouseHandler(BaseElement):
    """jessyInk mouse handler"""
    tag_name = 'jessyink:mousehandler'

class AddMouseHandler(JessyInkMixin, inkex.EffectExtension):
    """Add mouse handler"""
    def add_arguments(self, pars):
        pars.add_argument('--tab')
        pars.add_argument('--mouseSetting', default='default')

    def effect(self):
        self.is_installed()
        # Remove old mouse handler
        for node in self.svg.xpath("//jessyink:mousehandler"):
            node.delete()

        # Create new script node.
        script = Script()
        group = MouseHandler()

        if self.options.mouseSetting == "noclick":
            name = "noclick"
        elif self.options.mouseSetting == "draggingZoom":
            name = "zoomControl"
        elif self.options.mouseSetting == "default":
            # Default is to remove script and continue
            return

        with open(self.get_resource("jessyInk_core_mouseHandler_" + name + ".js")) as fhl:
            script.text = fhl.read()
        group.set("jessyink:subtype", "jessyInk_core_mouseHandler_" + name)

        group.append(script)
        self.svg.append(group)

if __name__ == '__main__':
    AddMouseHandler().run()
