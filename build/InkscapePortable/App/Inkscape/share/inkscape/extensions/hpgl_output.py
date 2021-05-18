#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2013 <Public Domain>
#               2018 Martin Owens <doctormo@gmail.com>
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

from __future__ import print_function

import inkex
from inkex.localization import inkex_gettext as _

import hpgl_encoder

class HpglOutput(inkex.OutputExtension):
    """Save as HPGL Output"""
    def add_arguments(self, pars):
        pars.add_argument('--tab')
        pars.add_argument('--resolutionX', type=float, default=1016.0, help='Resolution X (dpi)')
        pars.add_argument('--resolutionY', type=float, default=1016.0, help='Resolution Y (dpi)')
        pars.add_argument('--pen', type=int, default=1, help='Pen number')
        pars.add_argument('--force', type=int, default=24, help='Pen force (g)')
        pars.add_argument('--speed', type=int, default=20, help='Pen speed (cm/s)')
        pars.add_argument('--orientation', default='90', help='Rotation (Clockwise)')
        pars.add_argument('--mirrorX', type=inkex.Boolean, default=False, help='Mirror X axis')
        pars.add_argument('--mirrorY', type=inkex.Boolean, default=False, help='Mirror Y axis')
        pars.add_argument('--center', type=inkex.Boolean, default=False, help='Center zero point')
        pars.add_argument('--overcut', type=float, default=1.0, help='Overcut (mm)')
        pars.add_argument('--precut', type=inkex.Boolean, default=True, help='Use precut')
        pars.add_argument('--flat', type=float, default=1.2, help='Curve flatness')
        pars.add_argument('--autoAlign', type=inkex.Boolean, default=True, help='Auto align')
        pars.add_argument('--toolOffset', type=float, default=0.25,\
            help='Tool (Knife) offset correction (mm)')

    def save(self, stream):
        self.options.debug = False
        # get hpgl data
        encoder = hpgl_encoder.hpglEncoder(self)
        try:
            hpgl = encoder.getHpgl()
        except hpgl_encoder.NoPathError:
            raise inkex.AbortExtension(
                _("No paths were found. Please convert objects you want into paths."))
        # convert raw HPGL to HPGL
        hpgl_init = 'IN'
        if self.options.force > 0:
            hpgl_init += ';FS%d' % self.options.force
        if self.options.speed > 0:
            hpgl_init += ';VS%d' % self.options.speed
        hpgl = hpgl_init + hpgl + ';SP0;PU0,0;IN; '
        stream.write(hpgl.encode('utf-8'))


if __name__ == '__main__':
    HpglOutput().run()
