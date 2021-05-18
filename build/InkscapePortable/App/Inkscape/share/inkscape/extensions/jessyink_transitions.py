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
"""Effect to add transitions"""

import inkex
from inkex.styles import Style

from jessyink_install import JessyInkMixin, _

class Transitions(JessyInkMixin, inkex.EffectExtension):
    """Add transition to later"""
    def add_arguments(self, pars):
        pars.add_argument('--tab', dest='what')
        pars.add_argument('--layerName', default='')
        pars.add_argument('--effectIn', default='default')
        pars.add_argument('--effectOut', default='default')
        pars.add_argument('--effectInDuration', type=float, default=0.8)
        pars.add_argument('--effectOutDuration', type=float, default=0.8)

    def effect(self):
        self.is_installed()

        if not self.options.layerName:
            raise inkex.AbortExtension(_("Please enter a layer name."))

        node = self.svg.getElement("//*[@inkscape:groupmode='layer' "
                                   "and @inkscape:label='{}']".format(self.options.layerName))
        if node is None:
            raise inkex.AbortExtension(_("Layer '{}' not found.".format(self.options.layerName)))

        if self.options.effectIn == "default":
            node.set("jessyink:transitionIn", None)
        else:
            length = int(self.options.effectInDuration * 1000)
            node.set("jessyink:transitionIn", Style(name=self.options.effectIn, length=length))

        if self.options.effectOut == "default":
            node.set("jessyink:transitionOut", None)
        else:
            length = int(self.options.effectOutDuration * 1000)
            node.set("jessyink:transitionOut", Style(name=self.options.effectOut, length=length))

if __name__ == '__main__':
    Transitions().run()
