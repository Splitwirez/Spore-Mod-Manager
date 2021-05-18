#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2011 Nicolas Dufour (jazzynico)
# Direction code from the Restack extension, by Rob Antonishen
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
"""
Extract text and print it to the error console.
"""

from lxml.etree import tostring

import inkex
from inkex import TextElement, FlowRoot
from inkex.utils import KeyDict

# Old settings, supported because users click 'ok' without looking.
XAN = KeyDict({'l': 'left', 'r': 'right', 'm': 'center_x'})
YAN = KeyDict({'t': 'top', 'b': 'bottom', 'm': 'center_y'})

class Extract(inkex.EffectExtension):
    """Extract text and print out"""
    select_all = (TextElement, FlowRoot)

    def add_arguments(self, pars):
        pars.add_argument("-d", "--direction", default="tb", help="direction to extract text")
        pars.add_argument("-x", "--xanchor", default="center_x", help="horiz point to compare")
        pars.add_argument("-y", "--yanchor", default="center_y", help="vertical point to compare")

    def effect(self):
        # move them to the top of the object stack in this order.
        for node in sorted(self.svg.selection.get(TextElement, FlowRoot).values(), key=self._sort):
            self.recurse(node)

    def _sort(self, node):
        return node.bounding_box().get_anchor(
            self.options.xanchor, self.options.yanchor, self.options.direction)

    def recurse(self, node):
        """Go through each node and recusively self call for all children"""
        if node.text is not None or node.tail is not None:
            for child in node:
                if child.get('sodipodi:role'):
                    child.tail = "\n"
            inkex.errormsg(tostring(node, encoding='unicode', method='text').strip())
        else:
            for child in node:
                self.recurse(child)


if __name__ == '__main__':
    Extract().run()
