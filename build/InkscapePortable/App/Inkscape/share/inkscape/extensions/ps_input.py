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
Simple wrapper around ps2pdf
"""

import inkex
from inkex.command import call

class PostscriptInput(inkex.CallExtension):
    """Load Postscript/EPS Files by calling ps2pdf program"""
    input_ext = 'ps'
    output_ext = 'pdf'
    multi_inx = True

    def add_arguments(self, pars):
        pars.add_argument('--crop', type=inkex.Boolean, default=False)

    def call(self, input_file, output_file):
        crop = '-dEPSCrop' if self.options.crop else None
        call('ps2pdf', crop, input_file, output_file)

if __name__ == '__main__':
    PostscriptInput().run()
