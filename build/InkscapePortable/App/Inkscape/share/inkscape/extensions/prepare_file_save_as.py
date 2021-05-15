#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2014  Ryan Lerch     (multiple difference)
#              2016  Maren Hachmann <marenhachmannATyahoo.com> (refactoring, extend to multibool)
#              2017  Alvin Penner   <penner@vaxxine.com> (apply to 'File Save As...')
#
# This code is based on 'inkscape-extension-multiple-difference' by Ryan Lerch
# see : https://github.com/ryanlerch/inkscape-extension-multiple-difference
# also: https://github.com/Moini/inkscape-extensions-multi-bool
# It will call up a new instance of Inkscape and process the image there,
# so that the original file is left intact.
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
This extension will pre-process a vector image by applying the operations:
'EditSelectAllInAllLayers' and 'ObjectToPath'
before calling the dialog File->Save As....
"""

import inkex
from inkex.base import TempDirMixin
from inkex.command import inkscape_command
from inkex import load_svg

class PreProcess(TempDirMixin, inkex.EffectExtension):
    def effect(self):
        self.document = load_svg(inkscape_command(
            self.svg,
            verbs=['EditSelectAllInAllLayers', 'EditUnlinkClone', 'ObjectToPath'],
        ))

if __name__ == '__main__':
    PreProcess().run()
