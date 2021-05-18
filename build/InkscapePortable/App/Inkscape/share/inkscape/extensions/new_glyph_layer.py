#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2011 Felipe Correa da Silva Sanches
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

import locale
import sys

import inkex

class NewGlyphLayer(inkex.EffectExtension):
    def add_arguments(self, pars):
        self.arg_parser.add_argument("--text", default='', help="Unicode chars")

        self.encoding = sys.stdin.encoding
        if self.encoding == 'cp0' or self.encoding is None:
            self.encoding = locale.getpreferredencoding()

    def effect(self):
        # Get all the options
        unicode_chars = self.options.text
        if isinstance(unicode_chars, bytes):
            unicode_chars = unicode_chars.decode(self.encoding)

        # TODO: remove duplicate chars

        for char in unicode_chars:
            # Create a new layer.
            layer = self.svg.add(inkex.Layer.new(u'GlyphLayer-' + char))
            layer.set('style', 'display:none')  # initially not visible

            # TODO: make it optional ("Use current selection as template glyph")
            # Move selection to the newly created layer
            for node in self.svg.selected.values():
                layer.append(node)

if __name__ == '__main__':
    NewGlyphLayer().run()
