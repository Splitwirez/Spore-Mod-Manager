#!/usr/bin/env python
# coding=utf-8
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
"""
JessyInk Export to zipfile multiple layers.
"""

import zipfile

import inkex
from inkex.base import TempDirMixin
from inkex.command import take_snapshot

from jessyink_install import JessyInkMixin

class Export(JessyInkMixin, TempDirMixin, inkex.OutputExtension):
    """
    JessyInkExport Output Extension saves to a zipfile each of the layers.
    """
    dir_prefix = 'jessyInk-'

    def add_arguments(self, pars):
        pars.add_argument('--tab', type=str, dest='what')
        pars.add_argument('--type', type=str, dest='type', default='png')
        pars.add_argument('--resolution', type=int, default=96)

    def save(self, stream):
        self.is_installed()

        with zipfile.ZipFile(stream, "w", compression=zipfile.ZIP_STORED) as output:

            # Find layers.
            layers = self.svg.xpath("//svg:g[@inkscape:groupmode='layer']")

            if len(layers) < 1:
                inkex.errormsg("No layers found.")
                return

            for node in layers:
                # Make all layers invisible
                node.style['display'] = "none"

            for node in layers:
                # Show only one layer at a time.
                node.style.update("display:inherit;opacity:1")

                name = node.get('inkscape:label')
                newname = "{}.{}".format(name, self.options.type)
                filename = take_snapshot(self.document, dirname=self.tempdir,
                                         name=name, ext=self.options.type,
                                         dpi=self.options.resolution)
                output.write(filename, newname)

                node.style['display'] = "none"


if __name__ == '__main__':
    Export().run()
