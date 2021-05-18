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
#

import inkex
from inkex.ports import Serial
from inkex.localization import inkex_gettext as _

import hpgl_encoder

class Plot(inkex.EffectExtension):
    """Generate a plot in HPGL output"""
    def add_arguments(self, pars):
        pars.add_argument('--tab')
        pars.add_argument('--parallelPort', default='/dev/usb/lp2', help='Parallel port')
        pars.add_argument('--serialPort', default='COM1', help='Serial port')
        pars.add_argument('--serialBaudRate', default='9600', help='Serial Baud rate')
        pars.add_argument('--serialByteSize', default='eight', help='Serial byte size')
        pars.add_argument('--serialStopBits', default='one', help='Serial stop bits')
        pars.add_argument('--serialParity', default='none', help='Serial parity')
        pars.add_argument('--serialFlowControl', default='0', help='Flow control')
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
        pars.add_argument('--portType', type=self.arg_method('to'),\
            default=self.to_serial, dest="to_port", help='Port type')
        pars.add_argument('--commandLanguage', type=self.arg_method('convert'),\
            default=self.convert_hpgl, dest="to_language", help='Command Language Filter')

    def effect(self):
        # get hpgl data
        encoder = hpgl_encoder.hpglEncoder(self)
        try:
            self.options.to_port(self.options.to_language(encoder.getHpgl()))
        except hpgl_encoder.NoPathError:
            raise inkex.AbortExtension(_("No paths where found. Please convert objects to paths."))

    def convert_hpgl(self, hpgl):
        """Convert raw HPGL to HPGL"""
        init = 'IN'
        if self.options.force > 0:
            init += ';FS%d' % self.options.force
        if self.options.speed > 0:
            init += ';VS%d' % self.options.speed
        return init + hpgl + ';PU0,0;SP0;IN; '

    def convert_dmpl(self, hpgl):
        """Convert HPGL to DMPL"""
        # ;: = Initialise plotter
        # H = Home position
        # A = Absolute pen positioning
        # Ln = Line type
        # Pn = Pen select
        # Vn = velocity
        # ECn = Coordinate addressing, 1: 0.001 inch, 5: 0.005 inch, M: 0.1 mm
        # D = Pen down
        # U = Pen up
        # Z = Reset plotter
        # n,n, = Coordinate pair
        hpgl = hpgl.replace(';', ',')
        hpgl = hpgl.replace('SP', 'P')
        hpgl = hpgl.replace('PU', 'U')
        hpgl = hpgl.replace('PD', 'D')
        init = ';:HAL0'
        if self.options.speed > 0:
            init += 'V%d' % self.options.speed
        init += 'EC1'
        return init + hpgl[1:] + ',P0,U0,0,Z '

    def convert_knk(self, hpgl):
        """Convert HPGL to KNK Plotter Language"""
        init = 'ZG'
        if self.options.force > 0:
            init += ';FS%d' % self.options.force
        if self.options.speed > 0:
            init += ';VS%d' % self.options.speed
        return init + hpgl + ';SP0;PU0,0;@ '

    def to_parallel(self, hpgl):
        """Output to hgpl to a parallel port"""
        port = open(self.options.parallelPort, "wb")
        port.write(hpgl.encode('utf8'))
        port.close()

    def to_serial(self, hpgl):
        """Output to hgpl to a serial port"""
        with Serial(self.options.serialPort,
                    baud=self.options.serialBaudRate,
                    stop=self.options.serialStopBits,
                    size=self.options.serialByteSize,
                    flow=self.options.serialFlowControl,
                    parity=self.options.serialParity,
                   ) as comx:
            comx.write(hpgl.encode('utf8'))

if __name__ == '__main__':
    Plot().run()
