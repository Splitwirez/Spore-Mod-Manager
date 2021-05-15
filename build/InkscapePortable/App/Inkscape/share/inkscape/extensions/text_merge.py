#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2013 Nicolas Dufour (jazzynico)
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
Merge text blocks together.
"""

import inkex
from inkex.utils import KeyDict
from inkex import (
    Rectangle, FlowRoot, FlowPara, FlowRegion, TextElement, Tspan
)

# Old settings, supported because users click 'ok' without looking.
XAN = KeyDict({'l': 'left', 'r': 'right', 'm': 'center_x'})
YAN = KeyDict({'t': 'top', 'b': 'bottom', 'm': 'center_y'})

class Merge(inkex.EffectExtension):
    """Merge text blocks together"""
    def add_arguments(self, pars):
        pars.add_argument("-d", "--direction", default="tb", help="direction to merge text")
        pars.add_argument("-x", "--xanchor", default="center_x", help="horiz point to compare")
        pars.add_argument("-y", "--yanchor", default="center_y", help="vertical point to compare")
        pars.add_argument("-k", "--keepstyle", type=inkex.Boolean, help="keep format")
        pars.add_argument("-t", "--flowtext", type=inkex.Boolean,\
            help="use a flow text structure instead of a normal text element")

    def effect(self):
        if not self.svg.selected:
            for node in self.svg.xpath('//svg:text | //svg:flowRoot'):
                self.svg.selected[node.get('id')] = node

        if not self.svg.selected:
            return

        parentnode = self.svg.get_current_layer()

        if self.options.flowtext:
            text_element = FlowRoot
            text_span = FlowPara
        else:
            text_element = TextElement
            text_span = Tspan

        text_root = parentnode.add(text_element())
        text_root.set('xml:space', 'preserve')
        text_root.style = {
            'font-size': '20px',
            'font-style': 'normal',
            'font-weight': 'normal',
            'line-height': '125%',
            'letter-spacing': '0px',
            'word-spacing': '0px',
            'fill': '#000000',
            'fill-opacity': 1,
            'stroke': 'none'
        }

        for node in sorted(self.svg.selected.values(), key=self._sort):
            self.recurse(text_span, node, text_root)

        if self.options.flowtext:
            region = text_root.add(FlowRegion())
            region.set('xml:space', 'preserve')
            rect = region.add(Rectangle())
            rect.set('xml:space', 'preserve')
            rect.set('height', 200)
            rect.set('width', 200)

    def _sort(self, node):
        return node.bounding_box().get_anchor(
            self.options.xanchor, self.options.yanchor, self.options.direction)

    def recurse(self, text_span, node, span):
        """Recursively go through each node self calling on child nodes"""
        if not isinstance(node, FlowRegion):

            newspan = span.add(text_span())
            newspan.set('xml:space', 'preserve')

            newspan.set('sodipodi:role', node.get('sodipodi:role'))
            if isinstance(node, (TextElement, FlowPara)):
                newspan.set('sodipodi:role', 'line')

            if self.options.keepstyle:
                newspan.style = node.style

            if node.text is not None:
                newspan.text = node.text
            for child in node:
                self.recurse(text_span, child, newspan)
            if node.tail and not isinstance(node, TextElement):
                newspan.tail = node.tail


if __name__ == '__main__':
    Merge().run()
