#!/usr/bin/env python
#
# Copyright (C) 2016 su_v, <suv-sf@users.sf.net>
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
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
#
"""
Convert mesh gradient to path
"""

import inkex
from inkex.elements import MeshGradient

# globals
EPSILON = 1e-3
MG_PROPS = [
    'fill',
    'stroke'
]

def isclose(a, b, rel_tol=1e-09, abs_tol=0.0):
    """Test approximate equality.

    ref:
        PEP 485 -- A Function for testing approximate equality
        https://www.python.org/dev/peps/pep-0485/#proposed-implementation
    """
    # pylint: disable=invalid-name
    return abs(a-b) <= max(rel_tol * max(abs(a), abs(b)), abs_tol)


def reverse_path(csp):
    """Reverse path in CSP notation."""
    rcsp = []
    for subpath in reversed(csp):
        rsub = [list(reversed(cp)) for cp in reversed(subpath)]
        rcsp.append(rsub)
    return rcsp


def join_path(csp1, sp1, csp2, sp2):
    """Join sub-paths *sp1* and *sp2*."""
    pt1 = csp1[sp1][-1][1]
    pt2 = csp2[sp2][0][1]
    if (isclose(pt1[0], pt2[0], EPSILON) and
            isclose(pt1[1], pt2[1], EPSILON)):
        csp1[sp1][-1][2] = csp2[sp2][0][2]
        csp1[sp1].extend(csp2[sp2][1:])
    else:
        # inkex.debug('not close')
        csp1.append(csp2[sp2])
    return csp1


def is_url(val):
    """Check whether attribute value is linked resource."""
    return val.startswith('url(#')


def mesh_corners(meshgradient):
    """Return list of mesh patch corners, patch paths."""
    rows = len(meshgradient)
    cols = len(meshgradient[0])
    # first corner of mesh gradient
    corner_x = float(meshgradient.get('x', '0.0'))
    corner_y = float(meshgradient.get('y', '0.0'))
    # init corner and meshpatch lists
    corners = [[None for _ in range(cols+1)] for _ in range(rows+1)]
    corners[0][0] = [corner_x, corner_y]
    meshpatch_csps = []
    for meshrow in range(rows):
        for meshpatch in range(cols):
            # get start point for current meshpatch edges
            if meshrow == 0:
                first_corner = corners[meshrow][meshpatch]
            if meshrow > 0:
                first_corner = corners[meshrow][meshpatch+1]
            # parse path of meshpatch edges
            path = 'M {},{}'.format(*first_corner)
            for edge in meshgradient[meshrow][meshpatch]:
                path = ' '.join([path, edge.get('path')])
            csp = inkex.Path(path).to_superpath()
            # update corner list with current meshpatch
            if meshrow == 0:
                corners[meshrow][meshpatch+1] = csp[0][1][1]
                corners[meshrow+1][meshpatch+1] = csp[0][2][1]
                if meshpatch == 0:
                    corners[meshrow+1][meshpatch] = csp[0][3][1]
            if meshrow > 0:
                corners[meshrow][meshpatch+1] = csp[0][0][1]
                corners[meshrow+1][meshpatch+1] = csp[0][1][1]
                if meshpatch == 0:
                    corners[meshrow+1][meshpatch] = csp[0][2][1]
            # append to list of meshpatch csp
            meshpatch_csps.append(csp)
    return corners, meshpatch_csps


def mesh_hvlines(meshgradient):
    """Return lists of vertical and horizontal patch edges."""
    rows = len(meshgradient)
    cols = len(meshgradient[0])
    # init lists for horizontal, vertical lines
    hlines = [[None for _ in range(cols)] for _ in range(rows+1)]
    vlines = [[None for _ in range(rows)] for _ in range(cols+1)]
    for meshrow in range(rows):
        for meshpatch in range(cols):
            # horizontal edges
            if meshrow == 0:
                edge = meshgradient[meshrow][meshpatch][0]
                hlines[meshrow][meshpatch] = edge.get('path')
                edge = meshgradient[meshrow][meshpatch][2]
                hlines[meshrow+1][meshpatch] = edge.get('path')
            if meshrow > 0:
                edge = meshgradient[meshrow][meshpatch][1]
                hlines[meshrow+1][meshpatch] = edge.get('path')
            # vertical edges
            if meshrow == 0:
                edge = meshgradient[meshrow][meshpatch][1]
                vlines[meshpatch+1][meshrow] = edge.get('path')
                if meshpatch == 0:
                    edge = meshgradient[meshrow][meshpatch][3]
                    vlines[meshpatch][meshrow] = edge.get('path')
            if meshrow > 0:
                edge = meshgradient[meshrow][meshpatch][0]
                vlines[meshpatch+1][meshrow] = edge.get('path')
                if meshpatch == 0:
                    edge = meshgradient[meshrow][meshpatch][2]
                    vlines[meshpatch][meshrow] = edge.get('path')
    return hlines, vlines


def mesh_to_outline(corners, hlines, vlines):
    """Construct mesh outline as CSP path."""
    outline_csps = []
    path = 'M {},{}'.format(*corners[0][0])
    for edge_path in hlines[0]:
        path = ' '.join([path, edge_path])
    for edge_path in vlines[-1]:
        path = ' '.join([path, edge_path])
    for edge_path in reversed(hlines[-1]):
        path = ' '.join([path, edge_path])
    for edge_path in reversed(vlines[0]):
        path = ' '.join([path, edge_path])
    outline_csps.append(inkex.Path(path).to_superpath())
    return outline_csps


def mesh_to_grid(corners, hlines, vlines):
    """Construct mesh grid with CSP paths."""
    rows = len(corners) - 1
    cols = len(corners[0]) - 1
    gridline_csps = []
    # horizontal
    path = 'M {},{}'.format(*corners[0][0])
    for edge_path in hlines[0]:
        path = ' '.join([path, edge_path])
    gridline_csps.append(inkex.Path(path).to_superpath())
    for i in range(1, rows+1):
        path = 'M {},{}'.format(*corners[i][-1])
        for edge_path in reversed(hlines[i]):
            path = ' '.join([path, edge_path])
        gridline_csps.append(inkex.Path(path).to_superpath())
    # vertical
    path = 'M {},{}'.format(*corners[-1][0])
    for edge_path in reversed(vlines[0]):
        path = ' '.join([path, edge_path])
    gridline_csps.append(inkex.Path(path).to_superpath())
    for j in range(1, cols+1):
        path = 'M {},{}'.format(*corners[0][j])
        for edge_path in vlines[j]:
            path = ' '.join([path, edge_path])
        gridline_csps.append(inkex.Path(path).to_superpath())
    return gridline_csps


def mesh_to_faces(corners, hlines, vlines):
    """Construct mesh faces with CSP paths."""
    rows = len(corners) - 1
    cols = len(corners[0]) - 1
    face_csps = []
    for row in range(rows):
        for col in range(cols):
            # init new face
            face = []
            # init edge paths
            edge_t = hlines[row][col]
            edge_b = hlines[row+1][col]
            edge_l = vlines[col][row]
            edge_r = vlines[col+1][row]
            # top edge, first
            if row == 0:
                path = 'M {},{}'.format(*corners[row][col])
                path = ' '.join([path, edge_t])
                face.append(inkex.Path(path).to_superpath()[0])
            else:
                path = 'M {},{}'.format(*corners[row][col+1])
                path = ' '.join([path, edge_t])
                face.append(reverse_path(inkex.Path(path).to_superpath())[0])
            # right edge
            path = 'M {},{}'.format(*corners[row][col+1])
            path = ' '.join([path, edge_r])
            join_path(face, -1, inkex.Path(path).to_superpath(), 0)
            # bottom edge
            path = 'M {},{}'.format(*corners[row+1][col+1])
            path = ' '.join([path, edge_b])
            join_path(face, -1, inkex.Path(path).to_superpath(), 0)
            # left edge
            if col == 0:
                path = 'M {},{}'.format(*corners[row+1][col])
                path = ' '.join([path, edge_l])
                join_path(face, -1, inkex.Path(path).to_superpath(), 0)
            else:
                path = 'M {},{}'.format(*corners[row][col])
                path = ' '.join([path, edge_l])
                join_path(face, -1, reverse_path(inkex.Path(path).to_superpath()), 0)
            # append face to output list
            face_csps.append(face)
    return face_csps


class MeshToPath(inkex.EffectExtension):
    """Effect extension to convert mesh geometry to path data."""
    def add_arguments(self, pars):
        pars.add_argument("--tab", help="The selected UI-tab")
        pars.add_argument("--mode", default="outline", help="Edge mode")

    def process_props(self, mdict, res_type='meshgradient'):
        """Process style properties of style dict *mdict*."""
        result = []
        for key, val in mdict.items():
            if key in MG_PROPS:
                if is_url(val):
                    paint_server = self.svg.getElementById(val)
                    if res_type == 'meshgradient' and isinstance(paint_server, MeshGradient):
                        result.append(paint_server)
        return result

    def process_style(self, node, res_type='meshgradient'):
        """Process style of *node*."""
        result = []
        # Presentation attributes
        adict = dict(node.attrib)
        result.extend(self.process_props(adict, res_type))
        # Inline CSS style properties
        result.extend(self.process_props(node.style, res_type))
        # TODO: check for child paint servers
        return result

    def find_meshgradients(self, node):
        """Parse node style, return list with linked meshgradients."""
        return self.process_style(node, res_type='meshgradient')

    # ----- Process meshgradient definitions

    def mesh_to_csp(self, meshgradient):
        """Parse mesh geometry and build csp-based path data."""

        # init variables
        transform = None
        mode = self.options.mode

        # gradient units
        mesh_units = meshgradient.get('gradientUnits', 'objectBoundingBox')
        if mesh_units == 'objectBoundingBox':
            # TODO: position and scale based on "objectBoundingBox" units
            return

        # Inkscape SVG 0.92 and SVG 2.0 draft mesh transformations
        transform = meshgradient.gradientTransform * meshgradient.transform

        # parse meshpatches, calculate absolute corner coords
        corners, meshpatch_csps = mesh_corners(meshgradient)

        if mode == 'meshpatches':
            return meshpatch_csps, transform
        else:
            hlines, vlines = mesh_hvlines(meshgradient)
            if mode == 'outline':
                return mesh_to_outline(corners, hlines, vlines), transform
            elif mode == 'gridlines':
                return mesh_to_grid(corners, hlines, vlines), transform
            elif mode == 'faces':
                return mesh_to_faces(corners, hlines, vlines), transform

    # ----- Convert meshgradient definitions

    def csp_to_path(self, node, csp_list, transform=None):
        """Create new paths based on csp data, return group with paths."""
        # set up stroke width, group
        stroke_width = self.svg.unittouu('1px')
        stroke_color = '#000000'
        style = {
            'fill': 'none',
            'stroke': stroke_color,
            'stroke-width': str(stroke_width),
        }

        group = inkex.Group()
        # apply gradientTransform and node's preserved transform to group
        group.transform = transform * node.transform

        # convert each csp to path, append to group
        for csp in csp_list:
            elem = group.add(inkex.PathElement())
            elem.style = style
            elem.path = inkex.CubicSuperPath(csp)
            if self.options.mode == 'outline':
                elem.path.close()
            elif self.options.mode == 'faces':
                if len(csp) == 1 and len(csp[0]) == 5:
                    elem.path.close()
        return group

    def effect(self):
        """Main routine to convert mesh geometry to path data."""
        # loop through selection
        for node in self.svg.selected.values():
            meshgradients = self.find_meshgradients(node)
            # if style references meshgradient
            if meshgradients:
                for meshgradient in meshgradients:
                    csp_list = None
                    result = None
                    # parse mesh geometry
                    if meshgradient is not None:
                        csp_list, mat = self.mesh_to_csp(meshgradient)
                    # generate new paths with path data based on mesh geometry
                    if csp_list is not None:
                        result = self.csp_to_path(node, csp_list, mat)
                    # add result (group) to document
                    if result is not None:
                        index = node.getparent().index(node)
                        node.getparent().insert(index+1, result)


if __name__ == '__main__':
    MeshToPath().run()
