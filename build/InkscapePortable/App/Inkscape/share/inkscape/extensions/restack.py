#!/usr/bin/env python
#
# Copyright (C) 2007-2011 Rob Antonishen; rob.antonishen@gmail.com
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.
#

import random

import inkex
from inkex.utils import KeyDict
from inkex import SvgDocumentElement

# Old settings, supported because users click 'ok' without looking.
XAN = KeyDict({'l': 'left', 'r': 'right', 'm': 'center_x'})
YAN = KeyDict({'t': 'top', 'b': 'bottom', 'm': 'center_y'})
CUSTOM_DIRECTION = {270: 'tb', 90: 'bt', 0: 'lr', 360: 'lr', 180: 'rl'}

class Restack(inkex.EffectExtension):
    """Change the z-order of objects based on their position on the canvas"""
    restack_help = staticmethod(lambda: None)

    def add_arguments(self, pars):
        pars.add_argument("--tab", type=self.arg_method('restack'), default=self.restack_positional)
        pars.add_argument("--direction", default="tb", help="direction to restack")
        pars.add_argument("--angle", type=float, default=0.0, help="arbitrary angle")
        pars.add_argument("--xanchor", default="m", help="horizontal point to compare")
        pars.add_argument("--yanchor", default="m", help="vertical point to compare")
        pars.add_argument("--zsort", default="rev", help="Restack mode based on Z-Order")
        pars.add_argument("--nb_direction", default='', help='Direction tab')

    def effect(self):
        if not self.svg.selected:
            raise inkex.AbortExtension("There is no selection to restack.")

        # process selection to get list of objects to be arranged
        parentnode = None
        for node in self.svg.selection.filter(SvgDocumentElement):
            parentnode = node
            self.svg.set_selection(*list(node))

        if parentnode is None:
            parentnode = self.svg.get_current_layer()

        self.options.tab(parentnode)

    def restack_positional(self, parentnode):
        """Restack based on canvas position"""
        # move them to the top of the object stack in this order.
        for node in sorted(self.svg.selected.values(), key=self._sort):
            parentnode.append(node)
        return True

    def _sort(self, node):
        x, y = self.options.xanchor, self.options.yanchor
        selbox = self.svg.selection.bounding_box()
        direction = self.options.direction
        if 'custom' in self.options.nb_direction:
            direction = self.options.angle
        return node.bounding_box().get_anchor(x, y, direction, selbox)

    def restack_z_order(self, parentnode):
        """Restack based on z-order"""
        objects = list(self.svg.selected.values())
        if self.options.zsort == "rev":
            objects.reverse()
        elif self.options.zsort == "rand":
            random.shuffle(objects)
        if parentnode is not None:
            for item in objects:
                parentnode.append(item)
        return True

if __name__ == '__main__':
    Restack().run()
