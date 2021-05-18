#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2014 Martin Owens, email@doctormo.org
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
An extension to export multiple svg files from a single svg file containing layers.

Each defs is duplicated for each svg outputted.
"""

from __future__ import unicode_literals
import os
import sys
import copy
import tarfile
import io
import calendar
import time
import inkex

class TarLayers(inkex.OutputExtension):
    """Entry point to our layers export"""
    def make_template(self):
        """Returns the current document as a new empty document with the same defs"""
        newdoc = copy.deepcopy(self.document)
        for (name, layer) in self.layers(newdoc):
            layer.getparent().remove(layer)
        return newdoc

    def layers(self, document):
        for node in document.getroot().iterchildren():
            if isinstance(node, inkex.Layer) and node.label:
                yield (node.label, node)

    def io_document(self, name, doc):
        string = io.BytesIO()
        doc.write(string)
        info = tarfile.TarInfo(name=name+'.svg')
        info.mtime = calendar.timegm(time.gmtime())
        info.size  = string.tell()
        string.seek(0)
        return dict(tarinfo=info, fileobj=string)

    def save(self, stream):
        """Save the tar file output"""
        tar = tarfile.open(fileobj=stream, mode='w|')

        # Switch stdout to binary on Windows.
        if sys.platform == "win32":
            import msvcrt
            msvcrt.setmode(sys.stdout.fileno(), os.O_BINARY)

        template = self.make_template()

        previous = None
        for (name, _layer) in self.layers(self.document):
            layer = copy.deepcopy(_layer)
            if previous != None:
                template.getroot().replace(previous, layer)
            else:
                template.getroot().append(layer)
            previous = layer

            tar.addfile(**self.io_document(name, template))


if __name__ == '__main__':   #pragma: no cover
    TarLayers().run()
