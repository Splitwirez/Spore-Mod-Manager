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
"""Uninstall jessyInk"""

import inkex
from jessyink_install import JessyInkMixin

class Uninstall(JessyInkMixin, inkex.EffectExtension):
    """Uninstall jessyInk from this svg"""
    def add_arguments(self, pars):
        pars.add_argument('--tab')
        pars.add_argument('--remove_script', type=inkex.Boolean, default=True)
        pars.add_argument('--remove_effects', type=inkex.Boolean, default=True)
        pars.add_argument('--remove_masterSlide', type=inkex.Boolean, default=True)
        pars.add_argument('--remove_transitions', type=inkex.Boolean, default=True)
        pars.add_argument('--remove_autoTexts', type=inkex.Boolean, default=True)
        pars.add_argument('--remove_views', type=inkex.Boolean, default=True)

    def effect(self):
        # Remove script, if so desired.
        if self.options.remove_script:
            # Find and delete script node.
            for node in self.svg.xpath("//svg:script[@id='JessyInk']"):
                node.delete()

            # Remove "jessyInkInit()" in the "onload" attribute, if present.
            prop_list = []
            if self.svg.get("onload"):
                prop_list = self.prop_str_to_list(self.svg.get("onload"))

            for prop in prop_list:
                if prop == "jessyInkInit()":
                    prop_list.remove("jessyInkInit()")

            if prop_list:
                self.svg.set("onload", self.list_to_prop_str(prop_list))
            else:
                if self.document.getroot().get("onload"):
                    del self.document.getroot().attrib["onload"]

        self.attr_remove("effectIn", self.options.remove_effects)
        self.attr_remove("effectOut", self.options.remove_effects)
        self.attr_remove("masterSlide", self.options.remove_masterSlide)
        self.attr_remove("transitionIn", self.options.remove_transitions)
        self.attr_remove("transitionOut", self.options.remove_transitions)
        self.attr_remove("autoText", self.options.remove_autoTexts)
        self.attr_remove("view", self.options.remove_views)


# Create effect instance.
if __name__ == '__main__':
    Uninstall().run()
