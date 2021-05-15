#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2009 Karlisson Bezerra, contato@nerdson.com
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#

import inkex
from inkex import (
    TextElement, FlowRoot, FlowPara, Tspan, TextPath, Rectangle
)

class TextSplit(inkex.EffectExtension):
    """Split text up."""
    def add_arguments(self, pars):
        pars.add_argument("--tab", help="The selected UI-tab when OK was pressed")
        pars.add_argument("-s", "--splittype", default="word", help="type of split")
        pars.add_argument("-p", "--preserve", type=inkex.Boolean, default=True,\
            help="Preserve original")

    def split_lines(self, node):
        """Returns a list of lines"""
        lines = []
        count = 1

        for elem in node:
            if isinstance(elem, TextPath):
                inkex.errormsg("Text on path isn't supported. First remove text from path.")
                break
            elif not isinstance(elem, (FlowPara, Tspan)):
                continue

            text = TextElement(**node.attrib)

            # handling flowed text nodes
            if isinstance(node, FlowRoot):
                fontsize = node.style.get("font-size", "12px")
                fs = self.svg.unittouu(fontsize)

                # selects the flowRegion's child (svg:rect) to get @X and @Y
                flowref = node.findone('svg:flowRegion')[0]

                if isinstance(flowref, Rectangle):
                    text.set("x", flowref.get("x"))
                    text.set("y", str(float(flowref.get("y")) + fs * count))
                    count += 1
                else:
                    inkex.debug("This type of text element isn't supported. First unflow text.")
                    break

                # now let's convert flowPara into tspan
                tspan = Tspan()
                tspan.set("sodipodi:role", "line")
                tspan.text = elem.text
                text.append(tspan)

            else:
                from copy import copy
                x = elem.get("x") or node.get("x")
                y = elem.get("y") or node.get("y")

                text.set("x", x)
                text.set("y", y)
                text.append(copy(elem))

            lines.append(text)

        return lines

    def split_words(self, node):
        """Returns a list of words"""
        words = []

        # Function to recursively extract text
        def plain_str(elem):
            words = []
            if elem.text:
                words.append(elem.text)
            for n in elem:
                words.extend(plain_str(n))
                if n.tail:
                    words.append(n.tail)
            return words

        # if text has more than one line, iterates through elements
        lines = self.split_lines(node)
        if not lines:
            return words

        for line in lines:
            # gets the position of text node
            x = float(line.get("x"))
            y = line.get("y")

            # gets the font size. if element doesn't have a style attribute, it assumes font-size = 12px
            fontsize = line.style.get("font-size", "12px")
            fs = self.svg.unittouu(fontsize)

            # extract and returns a list of words
            words_list = "".join(plain_str(line)).split()
            prev_len = 0

            # creates new text nodes for each string in words_list
            for word in words_list:
                tspan = Tspan()
                tspan.text = word

                text = TextElement(**line.attrib)
                tspan.set('sodipodi:role', "line")

                # positioning new text elements
                x = x + prev_len * fs
                prev_len = len(word)
                text.set("x", str(x))
                text.set("y", str(y))

                text.append(tspan)
                words.append(text)

        return words

    def split_letters(self, node):
        """Returns a list of letters"""

        letters = []

        words = self.split_words(node)
        if not words:
            return letters

        for word in words:

            x = float(word.get("x"))
            y = word.get("y")

            # gets the font size. If element doesn't have a style attribute, it assumes font-size = 12px
            fontsize = word.style.get("font-size", "12px")
            fs = self.svg.unittouu(fontsize)

            # for each letter in element string
            for letter in word[0].text:
                tspan = Tspan()
                tspan.text = letter

                text = TextElement(**node.attrib)
                text.set("x", str(x))
                text.set("y", str(y))
                x += fs

                text.append(tspan)
                letters.append(text)
        return letters

    def effect(self):
        """Applies the effect"""

        split_type = self.options.splittype
        preserve = self.options.preserve

        # checks if the selected elements are text nodes
        for elem in self.svg.selection.get(TextElement, FlowRoot).values():
            if split_type == "line":
                nodes = self.split_lines(elem)
            elif split_type == "word":
                nodes = self.split_words(elem)
            elif split_type == "letter":
                nodes = self.split_letters(elem)

            for child in nodes:
                elem.getparent().append(child)

            # preserve original element
            if not preserve and nodes:
                parent = elem.getparent()
                parent.remove(elem)

if __name__ == '__main__':
    TextSplit().run()
