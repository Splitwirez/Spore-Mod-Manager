#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2008 Stephen Silver
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
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
#
"""
Simple wrapper around fig2dev
"""

import inkex
from inkex.command import call

class FigInput(inkex.CallExtension):
    """Load FIG Files by calling fig2dev program"""
    input_ext = 'fig'

    def call(self, input_file, output_file):
        call('fig2dev', '-L', 'svg', input_file, output_file)

if __name__ == '__main__':
    FigInput().run()
