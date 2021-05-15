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
"""Extension to sssign jessyInk views to objects"""

import inkex

from jessyink_install import JessyInkMixin, _

class View(JessyInkMixin, inkex.EffectExtension):
    """Assign jessyInk views to objects"""
    def add_arguments(self, pars):
        pars.add_argument('--tab', dest='what')
        pars.add_argument('--viewOrder', type=int, default=1)
        pars.add_argument('--viewDuration', type=float, default=0.8)
        pars.add_argument('--removeView', type=inkex.Boolean)

    def effect(self):
        self.is_installed()

        rect = self.svg.selected.first()

        if rect is None:
            raise inkex.AbortExtension(_("No object selected. Please select the object you want "
                                         "to assign a view to and then press apply.\n"))

        if not self.options.removeView:
            view_order = str(self.options.viewOrder)
            # Remove the view that currently has the requested order number.
            for node in rect.xpath("ancestor::svg:g[@inkscape:groupmode='layer']"
                                   "/descendant::*[@jessyink:view]"):
                prop_dict = inkex.Style(node.get("jessyink:view"))

                if prop_dict["order"] == view_order:
                    node.set("jessyink:view", None)

            # Set the new view.
            rect.set("jessyink:view", inkex.Style(
                name="view",
                order=view_order,
                length=int(self.options.viewDuration * 1000),
            ))

            # Remove possible effect arguments.
            self.attr_remove('effectIn')
            self.attr_remove('effectOut')
        else:
            self.attr_remove('view')

if __name__ == '__main__':
    View().run()
