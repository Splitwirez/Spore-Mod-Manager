#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2009 Kevin Lindsey, https://github.com/thelonious/DM2SVG
#               2011 Nikita Kitaev, https://github.com/nikitakit/DM2SVG
#                    Chris Morgan,  https://gist.github.com/1471691
#               2019 Martin Owens, <doctormo@gmail.com>
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
Import a DHW file from ACECAD DigiMemo, a hardware based digitiser
"""

import struct

import inkex
from inkex.utils import NSS
from inkex import AbortExtension, errormsg, Group, Polyline

NSS['dm'] = 'http://github.com/nikitakit/DM2SVG'

class DhwInput(inkex.InputExtension):
    """Open DHW files and convert to svg on the fly"""
    template = """<svg viewBox="0 0 {w} {h}"
  fill="none" stroke="black" stroke-width="10" stroke-linecap="round" stroke-linejoin="round"
  xmlns="http://www.w3.org/2000/svg"
  xmlns:svg="http://www.w3.org/2000/svg"
  xmlns:sodipodi="http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd"
  xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape"
  xmlns:dm="http://github.com/nikitakit/DM2SVG">
    <metadata>
      <dm:page
        version="{v}"
        width="{w}"
        height="{h}"
        page_type="{p}">
      </dm:page>
    </metadata>
    <rect width="{w}" height="{h}" fill="aliceblue"/>
    <g inkscape:groupmode="layer" id="layer1"></g>
</svg>"""

    def load(self, stream):
        """Load the steam as if it were an open DHW file"""
        header = list(struct.unpack('<32sBHHBxx', stream.read(40)))
        doc = header.pop(0).decode()
        if doc != 'ACECAD_DIGIMEMO_HANDWRITING_____':
            raise AbortExtension('Could not load file, not a ACECAD DHW file!')

        height = int(header[2])
        doc = self.get_template(**dict(zip(('v', 'w', 'h', 'p'), header)))
        svg = doc.getroot()

        timestamp = 0
        layer = svg.getElementById('layer1')

        while True:
            tag = stream.read(1)
            if tag == b'':
                break

            if ord(tag) <= 128:
                errormsg('Unsupported tag: {}\n'.format(tag))
                continue

            if tag == b'\x90':
                # New Layer element
                timestamp = 0
                name = 'layer{:d}'.format(ord(stream.read(1)) + 1)
                layer = svg.add(Group(inkscape_groupmode="layer", id=name))
            elif tag == b'\x88':
                # Read the timestamp next
                timestamp += ord(stream.read(1)) * 20
            else:
                # Pen down
                coords = [p for p in iter(lambda: read_point(stream, height), None)]
                # Pen up
                coords.append(read_point(stream, height))

                poly = layer.add(Polyline())
                poly.path = coords
                poly.set('dm:timestamp', timestamp)

        return doc


def read_point(stream, ymax):
    """If the next byte is a stop byte, return None. Otherwise read 4 bytes
    (in total) and return a 2D point.
    """
    # read first byte, it might be a stop byte
    x1 = struct.unpack('B', stream.read(1))[0]

    if x1 >= 0x80:
        return None

    x2, y1, y2 = struct.unpack('BBB', stream.read(3))

    x = x1 | x2 << 7
    y = y1 | y2 << 7

    return x, ymax - y

if __name__ == '__main__':
    DhwInput().run()
