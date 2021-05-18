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
"""
Jessyink Set Master Slide
"""

import inkex

from jessyink_install import JessyInkMixin, _

class MasterSlide(JessyInkMixin, inkex.EffectExtension):
    """Effect Extension for master slide"""
    def add_arguments(self, pars):
        self.arg_parser.add_argument('--tab')
        self.arg_parser.add_argument('--layerName', default='')

    def effect(self):
        self.is_installed()
        # Remove old master slide property
        for node in self.svg.xpath("//*[@jessyink:masterSlide='masterSlide']"):
            node.set("jessyink:masterSlide", None)

        # Set new master slide.
        if self.options.layerName != "":
            nodes = self.svg.xpath(("//*[@inkscape:groupmode='layer' "
                                   "and @inkscape:label='{self.options.layerName}']").format(**locals()))
            if not nodes:
                inkex.errormsg(_("Layer not found. Removed current master slide selection.\n"))
            elif len(nodes) > 1:
                inkex.errormsg(_("More than one layer with this name found. "
                                 "Removed current master slide selection.\n"))
            else:
                nodes[0].set("jessyink:masterSlide", "masterSlide")

if __name__ == '__main__':
    MasterSlide().run()
