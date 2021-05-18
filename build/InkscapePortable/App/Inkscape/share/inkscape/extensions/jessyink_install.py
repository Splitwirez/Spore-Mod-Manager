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
"""Install jessyInk scripts"""

import inkex
from inkex import Script
from inkex.utils import NSS

from inkex.localization import inkex_gettext as _

NSS[u"jessyink"] = u"https://launchpad.net/jessyink"

class JessyInkMixin(object):
    """Common jessyInk items"""
    def is_installed(self):
        """Check jessyInk is installed correctly"""
        scripts = self.svg.getElement("//svg:script[@jessyink:version='1.5.5']")
        if scripts is None:
            raise inkex.AbortExtension(_(
                "The JessyInk script is not installed in this SVG file or has a "
                "different version than the JessyInk extensions. Please select "
                "\"install/update...\" from the \"JessyInk\" sub-menu of the \"Extensions\" "
                "menu to install or update the JessyInk script.\n\n"))

    def attr_remove(self, prop, is_removed=True):
        """Remove a property if it exists in the svg"""
        if is_removed:
            for node in self.svg.xpath("//*[@jessyink:{}]".format(prop)):
                node.set("jessyink:{}".format(prop), None)
            for node in self.svg.xpath("//*[@jessyInk_{}]".format(prop)):
                node.set("jessyInk_{}".format(prop), None)

    def attr_update(self, name):
        """Update a single attr"""
        for node in self.svg.xpath("//*[@jessyInk_{}]".format(name)):
            node.set("jessyink:{}".format(name), node.get("jessyInk_{}".format(name)))
            node.set("jessyInk_{}".format(name), None)
        for node in self.svg.xpath("//*[@jessyink:{}]".format(name)):
            node.set("jessyink:{}".format(name), node.get("jessyink:{}".format(name)).replace("=", ":"))

    @staticmethod
    def prop_str_to_list(string):
        """Script string to list of instructions"""
        return [prop.strip() for prop in string.split(";") if prop]

    @staticmethod
    def list_to_prop_str(lst):
        """List of instructions to script string"""
        return "; ".join(lst) + ';'


class Install(JessyInkMixin, inkex.EffectExtension):
    """Install jessyInk extension into an SVG"""
    def add_arguments(self, pars):
        pars.add_argument('--tab', type=str, dest='what')

    def effect(self):
        # Find and delete old script node
        for node in self.svg.xpath("//svg:script[@id='JessyInk']"):
            node.getparent().remove(node)

        # Create new script node
        script_elem = Script()
        with open(self.get_resource("jessyInk.js")) as fhl:
            script_elem.text = fhl.read()
        script_elem.set("id", "JessyInk")
        script_elem.set("jessyink:version", '1.5.5')
        self.svg.append(script_elem)

        # Remove "jessyInkInit()" in the "onload" attribute, if present.
        prop_list = [prop.strip() for prop in self.svg.get("onload", '').split(';')]
        if "jessyInkInit()" in prop_list:
            prop_list.remove("jessyInkInit()")
        self.svg.set("onload", "; ".join(prop_list) or None)

        # Update jessyInk attributes to new formats
        for attr in ('effectIn', 'effectOut', 'masterSlide',
                     'transitionIn', 'transitionOut', 'autoText'):
            self.attr_update(attr)


# Create effect instance
if __name__ == '__main__':
    Install().run()
