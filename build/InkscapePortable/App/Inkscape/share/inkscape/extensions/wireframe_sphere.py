#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2009 John Beard john.j.beard@gmail.com
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
This extension renders a wireframe sphere constructed from lines of latitude
and lines of longitude.

The number of lines of latitude and longitude is independently variable. Lines
of latitude and longtude are in separate subgroups. The whole figure is also in
its own group.

The whole sphere can be tilted towards or away from the veiwer by a given
number of degrees. If the whole sphere is then rotated normally in Inkscape,
any position can be achieved.

There is an option to hide the lines at the back of the sphere, as if the
sphere were opaque.
"""
# FIXME: Lines of latitude only have an approximation of the function needed
#            to hide the back portion. If you can derive the proper equation,
#            please add it in.
#            Line of longitude have the exact method already.
#            Workaround: Use the Inkscape ellipse tool to edit the start and end
#            points of the lines of latitude to end at the horizon circle.
#
# TODO:  Add support for odd numbers of lines of longitude. This means breaking
#        the line at the poles, and having two half ellipses for each line.
#        The angles at which the ellipse arcs pass the poles are not constant and
#        need to be derived before this can be implemented.
# TODO:  Add support for prolate and oblate spheroids
#
#    0.10    2009-10-25  First version. Basic spheres supported.
#                        Hidden lines of latitude still not properly calculated.
#                        Prolate and oblate spheroids not considered.

from math import acos, atan, cos, pi, sin, tan

import inkex

# add a tiny value to the ellipse radii, so that if we get a
# zero radius, the ellipse still shows up as a line
EPSILON = 0.001


class WireframeSphere(inkex.GenerateExtension):
    """Writeframe extension, generate a wireframe"""
    container_label = 'WireframeSphere'

    def container_transform(self):
        transform = super(WireframeSphere, self).container_transform()
        if self.options.TILT < 0:
            transform *= inkex.Transform(scale=(1, -1))
        return transform

    def add_arguments(self, pars):
        pars.add_argument("--num_lat", type=int, dest="NUM_LAT", default=19)
        pars.add_argument("--num_long", type=int, dest="NUM_LONG", default=24)
        pars.add_argument("--radius", type=float, dest="RADIUS", default=100.0)
        pars.add_argument("--tilt", type=float, dest="TILT", default=35.0)
        pars.add_argument("--rotation", type=float, dest="ROT_OFFSET", default=4)
        pars.add_argument("--hide_back", type=inkex.Boolean, dest="HIDE_BACK", default=False)

    def generate(self):
        opt = self.options

        # PARAMETER PROCESSING
        if opt.NUM_LONG % 2 != 0:  # lines of longitude are odd : abort
            inkex.errormsg('Please enter an even number of lines of longitude.')
            return

        radius = self.svg.unittouu(str(opt.RADIUS) + 'px')
        tilt = abs(opt.TILT) * (pi / 180)  # Convert to radians
        rotate = opt.ROT_OFFSET * pi / 180  # Convert to radians

        # only process longitudes if we actually want some
        if opt.NUM_LONG > 0:
            # Yieled elements are added to generated container
            yield self.longitude_lines(opt.NUM_LONG, tilt, radius, rotate)

        if opt.NUM_LAT > 0:
            # Yieled elements are added to generated container
            # Account for the fact that we loop over N-1 elements
            yield self.latitude_lines(opt.NUM_LAT + 1, tilt, radius)

        # THE HORIZON CIRCLE - circle, centred on the sphere centre
        yield self.draw_ellipse((radius, radius), (0, 0))

    def longitude_lines(self, number, tilt, radius, rotate):
        """Add lines of latitude as a group"""
        # GROUP FOR THE LINES OF LONGITUDE
        grp_long = inkex.Group()
        grp_long.set('inkscape:label', 'Lines of Longitude')

        # angle between neighbouring lines of longitude in degrees
        #delta_long = 360.0 / number

        for i in range(0, number // 2):
            # The longitude of this particular line in radians
            long_angle = rotate + (i * (360.0 / number)) * (pi / 180.0)
            if long_angle > pi:
                long_angle -= 2 * pi
            # the rise is scaled by the sine of the tilt
            # length     = sqrt(width*width+height*height)  #by pythagorean theorem
            # inverse    = sin(acos(length/so.RADIUS))
            inverse = abs(sin(long_angle)) * cos(tilt)

            rads = (radius * inverse + EPSILON, radius)

            # The rotation of the ellipse to get it to pass through the pole (degs)
            rotation = atan(
                (radius * sin(long_angle) * sin(tilt)) /
                (radius * cos(long_angle))
            ) * (180.0 / pi)

            # remove the hidden side of the ellipses if required
            # this is always exactly half the ellipse, but we need to find out which half
            start_end = (0, 2 * pi)  # Default start and end angles -> full ellipse
            if self.options.HIDE_BACK:
                if long_angle <= pi / 2:  # cut out the half ellispse that is hidden
                    start_end = (pi / 2, 3 * pi / 2)
                else:
                    start_end = (3 * pi / 2, pi / 2)

            # finally, draw the line of longitude
            # the centre is always at the centre of the sphere
            elem = grp_long.add(self.draw_ellipse(rads, (0, 0), start_end))
            # the rotation will be applied about the group centre (the centre of the sphere)
            elem.transform = inkex.Transform(rotate=(rotation,))
        return grp_long

    def latitude_lines(self, number, tilt, radius):
        """Add lines of latitude as a group"""
        # GROUP FOR THE LINES OF LATITUDE
        grp_lat = inkex.Group()
        grp_lat.set('inkscape:label', 'Lines of Latitude')

        # Angle between the line of latitude (subtended at the centre)
        delta_lat = 180.0 / number

        for i in range(1, number):
            # The angle of this line of latitude (from a pole)
            lat_angle = ((delta_lat * i) * (pi / 180))

            # The width of the LoLat (no change due to projection)
            # The projected height of the line of latitude
            rads = (
                radius * sin(lat_angle), # major
                (radius * sin(lat_angle) * sin(tilt)) + EPSILON, # minor
            )

            # The x position is the sphere center, The projected y position of the LoLat
            pos = (0, radius * cos(lat_angle) * cos(tilt))

            if self.options.HIDE_BACK:
                if lat_angle > tilt:  # this LoLat is partially or fully visible
                    if lat_angle > pi - tilt:  # this LoLat is fully visible
                        grp_lat.add(self.draw_ellipse(rads, pos))
                    else:  # this LoLat is partially visible
                        proportion = -(acos(tan(lat_angle - pi / 2) \
                                       / tan(pi / 2 - tilt))) / pi + 1
                        # make the start and end angles (mirror image around pi/2)
                        start_end = (pi / 2 - proportion * pi, pi / 2 + proportion * pi)
                        grp_lat.add(self.draw_ellipse(rads, pos, start_end))

            else:  # just draw the full lines of latitude
                grp_lat.add(self.draw_ellipse(rads, pos))
        return grp_lat

    def draw_ellipse(self, r_xy, c_xy, start_end=(0, 2 * pi)):
        """Creates an elipse with all the required sodipodi attributes"""
        path = inkex.PathElement()
        path.update(**{
            'style': {'stroke': '#000000',
                      'stroke-width': str(self.svg.unittouu('1px')),
                      'fill': 'none'},
            'sodipodi:cx': str(c_xy[0]),
            'sodipodi:cy': str(c_xy[1]),
            'sodipodi:rx': str(r_xy[0]),
            'sodipodi:ry': str(r_xy[1]),
            'sodipodi:start': str(start_end[0]),
            'sodipodi:end': str(start_end[1]),
            'sodipodi:open': 'true',  # all ellipse sectors we will draw are open
            'sodipodi:type': 'arc',
        })
        return path

if __name__ == '__main__':
    WireframeSphere().run()
