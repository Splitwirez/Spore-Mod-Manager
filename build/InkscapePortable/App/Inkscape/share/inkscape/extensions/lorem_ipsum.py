#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2006 Jos Hirth, kaioa.com
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
"""
Example filltext sentences generated over at http://lipsum.com/
"""

import random

import inkex
from inkex import Layer, FlowRoot, FlowRegion, FlowPara, Rectangle

CORPA = [
    'Lorem ipsum dolor sit amet, consectetuer adipiscing elit. ',
    'Duis sem velit, ultrices et, fermentum auctor, rhoncus ut, ligula. ',
    'Phasellus at purus sed purus cursus iaculis. ',
    'Suspendisse fermentum. ',
    'Pellentesque et arcu. ',
    'Maecenas viverra. ',
    'In consectetuer, lorem eu lobortis egestas, velit odio imperdiet'
    ' eros, sit amet sagittis nunc mi ac neque. ',
    'Sed non ipsum. ',
    'Nullam venenatis gravida orci. ',
    'Curabitur nunc ante, ullamcorper vel, auctor a, aliquam at, tortor. ',
    'Etiam sodales orci nec ligula. ',
    'Sed at turpis vitae velit euismod aliquet. ',
    'Fusce venenatis ligula in pede. ',
    'Pellentesque viverra dolor non nunc. ',
    'Donec interdum vestibulum libero. ',
    'Morbi volutpat. ',
    'Phasellus hendrerit. ',
    'Quisque dictum quam vel neque. ',
    'Quisque aliquam, nulla ac scelerisque convallis, nisi ligula sagittis'
    ' risus, at nonummy arcu urna pulvinar nibh. ',
    'Nam pharetra. ',
    'Nam rhoncus, lectus vel hendrerit congue, nisl lorem feugiat ante, in'
    ' fermentum erat nulla tristique arcu. ',
    'Mauris et dolor. ',
    'Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere'
    ' cubilia Curae; Donec gravida, ante vel ornare lacinia, orci enim porta'
    ' est, eget sollicitudin lectus lectus eget lacus. ',
    'Praesent a lacus vitae turpis consequat semper. ',
    'In commodo, dolor quis fermentum ullamcorper, urna massa volutpat'
    ' massa, vitae mattis purus arcu nec nulla. ',
    'In hac habitasse platea dictumst. ',
    'Praesent scelerisque. ',
    'Nullam sapien mauris, venenatis at, fermentum at, tempus eu, urna. ',
    'Vestibulum non arcu a ante feugiat vestibulum. ',
    'Nam laoreet dui sed magna. ',
    'Proin diam augue, semper vitae, varius et, viverra id, felis. ',
    'Pellentesque sit amet dui vel justo gravida auctor. ',
    'Aenean scelerisque metus eget sem. ',
    'Maecenas rhoncus rhoncus ipsum. ',
    'Donec nonummy lacinia leo. ',
    'Aenean turpis ipsum, rhoncus vitae, posuere vitae, euismod sed, ligula. ',
    'Pellentesque habitant morbi tristique senectus et netus et malesuada'
    ' fames ac turpis egestas. ',
    'Mauris tempus diam. ',
    'Maecenas justo. ',
    'Sed a lorem ut est tincidunt consectetuer. ',
    'Ut eu metus id lectus vestibulum ultrices. ',
    'Suspendisse lectus. ',
    'Vivamus posuere, ante eu tempor dictum, felis nibh facilisis sem, eu'
    ' auctor metus nulla non lorem. ',
    'Suspendisse potenti. ',
    'Integer fringilla. ',
    'Morbi urna. ',
    'Morbi pulvinar nulla sit amet nisl. ',
    'Mauris urna sem, suscipit vitae, dignissim id, ultrices sed, nunc. ',
    'Morbi a mauris. ',
    'Pellentesque suscipit accumsan massa. ',
    'Quisque arcu ante, cursus in, ornare quis, viverra ut, justo. ',
    'Quisque facilisis, urna sit amet pulvinar mollis, purus arcu adipiscing'
    ' velit, non condimentum diam purus eu massa. ',
    'Suspendisse potenti. ',
    'Phasellus nisi metus, tempus sit amet, ultrices ac, porta nec, felis. ',
    'Aliquam metus. ',
    'Nam a nunc. ',
    'Vivamus feugiat. ',
    'Nunc metus. ',
    'Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere'
    ' cubilia Curae; Vivamus eu orci. ',
    'Sed elementum, felis quis porttitor sollicitudin, augue nulla sodales'
    ' sapien, sit amet posuere quam purus at lacus. ',
    'Curabitur tincidunt tellus nec purus. ',
    'Nam consectetuer mollis dolor. ',
    'Sed quis elit. ',
    'Aenean luctus vulputate turpis. ',
    'Proin lectus orci, venenatis pharetra, egestas id, tincidunt vel, eros. ',
    'Nulla facilisi. ',
    'Aliquam vel nibh. ',
    'Vivamus nisi elit, nonummy id, facilisis non, blandit ac, dolor. ',
    'Etiam cursus purus interdum libero. ',
    'Nam id neque. ',
    'Etiam pede nunc, vestibulum vel, rutrum et, tincidunt eu, enim. ',
    'Aenean id purus. ',
    'Aenean ultrices turpis. ',
    'Mauris et pede. ',
    'Suspendisse potenti. ',
    'Aliquam velit dui, commodo quis, porttitor eget, convallis et, nisi. ',
    'Maecenas convallis dui. ',
    'In leo ante, venenatis eu, volutpat ut, imperdiet auctor, enim. ',
    'Mauris ac massa vestibulum nisl facilisis viverra. ',
    'Phasellus magna sem, vulputate eget, ornare sed, dignissim sit amet, pede. ',
    'Aenean justo ipsum, luctus ut, volutpat laoreet, vehicula in, libero. ',
    'Praesent semper, neque vel condimentum hendrerit, lectus elit pretium'
    'ligula, nec consequat nisl velit at dui. ',
    'Proin dolor sapien, adipiscing id, sagittis eu, molestie viverra, mauris. ',
    'Aenean ligula. ',
    'Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere'
    ' cubilia Curae; Suspendisse potenti. ',
    'Etiam pharetra lacus sed velit imperdiet bibendum. ',
    'Nunc in turpis ac lacus eleifend sagittis. ',
    'Nam massa turpis, nonummy et, consectetuer id, placerat ac, ante. ',
    'In tempus urna. ',
    'Quisque vehicula porttitor odio. ',
    'Aliquam sed erat. ',
    'Vestibulum viverra varius enim. ',
    'Donec ut purus. ',
    'Pellentesque convallis dolor vel libero. ',
    'Integer tempus malesuada pede. ',
    'Integer porta. ',
    'Donec diam eros, tristique sit amet, pretium vel, pellentesque ut, neque. ',
    'Nulla blandit justo a metus. ',
    'Curabitur accumsan felis in erat. ',
    'Curabitur lorem risus, sagittis vitae, accumsan a, iaculis id, metus. ',
    'Nulla sagittis condimentum ligula. ',
    'Aliquam imperdiet lobortis metus. ',
    'Suspendisse molestie sem. ',
    'Ut venenatis. ',
    'Pellentesque condimentum felis a sem. ',
    'Fusce nonummy commodo dui. ',
    'Nullam libero nunc, tristique eget, laoreet eu, sagittis id, ante. ',
    'Etiam fermentum. ',
    'Phasellus auctor enim eget sem. ',
    'Morbi turpis arcu, egestas congue, condimentum quis, tristique cursus, leo. ',
    'Sed fringilla. ',
    'Nam malesuada sapien eu nibh. ',
    'Pellentesque ac turpis. ',
    'Nulla sed lacus. ',
    'Mauris sed nulla quis nisi interdum tempor. ',
    'Quisque pretium rutrum ligula. ',
    'Mauris tempor ultrices justo. ',
    'In hac habitasse platea dictumst. ',
    'Donec sit amet enim. ',
    'Suspendisse venenatis. ',
    'Nam nisl quam, posuere non, volutpat sed, semper vitae, magna. ',
    'Donec ut urna. ',
    'Integer risus velit, facilisis eget, viverra et, venenatis id, leo. ',
    'Cras facilisis felis sit amet lorem. ',
    'Nam molestie nisl at metus. ',
    'Suspendisse viverra placerat tortor. ',
    'Phasellus lacinia iaculis mi. ',
    'Sed dolor. ',
    'Quisque malesuada nulla sed pede volutpat pulvinar. ',
    'Cras gravida. ',
    'Mauris tincidunt aliquam ante. ',
    'Fusce consectetuer tellus ut nisl. ',
    'Curabitur risus urna, placerat et, luctus pulvinar, auctor vel, orci. ',
    'Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos hymenaeos. ',
    'Praesent aliquet, neque pretium congue mattis, ipsum augue dignissim ante, ac'
    ' pretium nisl lectus at magna. ',
    'Vivamus quis mi. ',
    'Nam sed nisl nec elit suscipit ullamcorper. ',
    'Donec tempus quam quis neque. ',
    'Donec rutrum venenatis dui. ',
    'Praesent a eros. ',
    'Aliquam justo lectus, iaculis a, auctor sed, congue in, nisl. ',
    'Etiam non neque ac mi vestibulum placerat. ',
    'Donec at diam a tellus dignissim vestibulum. ',
    'Integer accumsan. ',
    'Cras ac enim vel dui vestibulum suscipit. ',
    'Pellentesque tempor. ',
    'Praesent lacus. '
]


class LoremIpsum(inkex.EffectExtension):
    """Generate text with psuedo latin content"""
    def add_arguments(self, pars):
        pars.add_argument("--num", type=int, default=5, help="Number of paragraphs to generate")
        pars.add_argument("-c", "--sentencecount", type=int, default=16,
                          help="Number of Sentences")
        pars.add_argument("-f", "--fluctuation", type=int, default=4, help="+/-")
        pars.add_argument("--tab", help="The selected UI-tab when OK was pressed")

    def make_paragraph(self, text_index=0):
        """Make a paragraph"""
        _min = max(1, self.options.sentencecount - self.options.fluctuation)
        _max = max(2, self.options.sentencecount + self.options.fluctuation)
        scount = int(random.random() * _max + _min)
        for sentence in range(scount):
            if text_index + sentence == 0:
                yield CORPA[0]
            else:
                index = int(random.random() * (len(CORPA) - 1))
                yield CORPA[index]

    def add_text(self, node):
        """Create many flowed text paragraph and append to node"""
        for text_index in range(self.options.num):
            para = node.add(FlowPara())
            para.text = ''.join(self.make_paragraph(text_index))
            node.append(FlowPara())

    def effect(self):
        # Existing text flow to insert new text into
        for node in self.svg.selection.filter(FlowRoot):
            self.add_text(node)
            return

        # New text layer with lorum ipsum content
        root = FlowRoot()
        root.set('xml:space', 'preserve')
        region = root.add(FlowRegion())

        shape = self.svg.selection.first()
        if shape is not None:
            parent = shape.getparent()
            region.add(shape.copy())
        else:
            parent = self.svg.add(Layer.new('lorum ipsum'))
            region.add(Rectangle(x='0', y='0',\
                width=str(int(self.svg.width)),\
                height=str(int(self.svg.height))))

        parent.add(root)
        self.add_text(root)


if __name__ == '__main__':
    LoremIpsum().run()
