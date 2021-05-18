#!/usr/bin/env python
#
# Copyright 2008, 2009 Hannes Hochreiner
#                 2020 Martin Owens
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
"""Jessyink effect extension."""

import inkex

from jessyink_install import JessyInkMixin, _

class JessyinkEffects(JessyInkMixin, inkex.EffectExtension):
    """Add ad effect to jessy ink selected items"""
    def add_arguments(self, pars):
        pars.add_argument('--tab')
        pars.add_argument('--effectInOrder', type=int, default=1)
        pars.add_argument('--effectInDuration', type=float, default=0.8)
        pars.add_argument('--effectIn', default='none')
        pars.add_argument('--effectOutOrder', type=int, default=2)
        pars.add_argument('--effectOutDuration', type=float, default=0.8)
        pars.add_argument('--effectOut', default='none')

    def effect(self):
        self.is_installed()
        if not self.svg.selected:
            raise inkex.AbortExtension(
                _("No object selected. Please select the object you want to "
                  "assign an effect to and then press apply.\n"))

        for elem in self.svg.selected.values():
            self._process(elem, 'effectIn')
            self._process(elem, 'effectOut')

    def _process(self, elem, name):
        effect = getattr(self.options, name)
        order = getattr(self.options, name + 'Order')
        duration = int(getattr(self.options, name + 'Duration') * 1000)

        if effect in ("appear", "fade", "pop"):
            elem.set("jessyink:" + name, inkex.Style(name=effect, order=order, length=duration))
            # Remove possible view argument.
            elem.pop('jessyink:view', None)
        else:
            elem.pop('jessyink:' + name, None)

if __name__ == '__main__':
    JessyinkEffects().run()
