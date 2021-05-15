#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005,2007 Aaron Spike, aaron@ekips.org
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
# pylint: disable=ungrouped-imports
"""
Embed images so they are base64 encoded data inside the svg.
"""

from __future__ import unicode_literals

import os

import inkex
from inkex import Image

try:
    import urllib.request as urllib
    import urllib.parse as urlparse
    from base64 import encodebytes
except ImportError:
    # python2 compatibility, remove when python3 only.
    import urllib
    import urlparse
    from base64 import encodestring as encodebytes

class EmbedImage(inkex.EffectExtension):
    """Allow selected image tags to become embeded image tags"""
    def add_arguments(self, pars):
        pars.add_argument("--selectedonly", type=inkex.Boolean, help="embed only selected images")

    def effect(self):
        # if slectedonly is enabled and there is a selection
        # only embed selected images. otherwise embed all images
        if self.options.selectedonly:
            images = self.svg.selection.get(Image)
        else:
            images = self.svg.xpath('//svg:image')

        for node in images:
            self.embed_image(node)

    def embed_image(self, node):
        """Embed the data of the selected Image Tag element"""
        xlink = node.get('xlink:href')
        if xlink and xlink[:5] == 'data:':
            # No need, data alread embedded
            return

        url = urlparse.urlparse(xlink)
        href = urllib.url2pathname(url.path)

        # Primary location always the filename itself.
        path = self.absolute_href(href or '')

        # Backup directory where we can find the image
        if not os.path.isfile(path):
            path = node.get('sodipodi:absref', path)

        if not os.path.isfile(path):
            inkex.errormsg(_('File not found "{}". Unable to embed image.').format(path))
            return

        with open(path, "rb") as handle:
            # Don't read the whole file to check the header
            file_type = get_type(path, handle.read(10))
            handle.seek(0)

            if file_type:
                # Future: Change encodestring to encodebytes when python3 only
                node.set('xlink:href', 'data:{};base64,{}'.format(
                    file_type, encodebytes(handle.read()).decode('ascii')))
                node.pop('sodipodi:absref')
            else:
                inkex.errormsg(_("%s is not of type image/png, image/jpeg, "\
                    "image/bmp, image/gif, image/tiff, or image/x-icon") % path)


def get_type(path, header):
    """Basic magic header checker, returns mime type"""
    for head, mime in (
            (b'\x89PNG', 'image/png'),
            (b'\xff\xd8', 'image/jpeg'),
            (b'BM', 'image/bmp'),
            (b'GIF87a', 'image/gif'),
            (b'GIF89a', 'image/gif'),
            (b'MM\x00\x2a', 'image/tiff'),
            (b'II\x2a\x00', 'image/tiff'),
        ):
        if header.startswith(head):
            return mime

    # ico files lack any magic... therefore we check the filename instead
    for ext, mime in (
            # official IANA registered MIME is 'image/vnd.microsoft.icon' tho
            ('.ico', 'image/x-icon'),
            ('.svg', 'image/svg+xml'),
        ):
        if path.endswith(ext):
            return mime
    return None

if __name__ == '__main__':
    EmbedImage().run()
