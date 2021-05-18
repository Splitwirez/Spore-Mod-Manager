#!/usr/bin/env python

# Copyright (C) 2019 Gemy Cedric Activdesign.eu
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
import inkwebeffect
import inkex

class InteractiveMockup(inkwebeffect.InkWebEffect):
    def add_arguments(self, pars):
        pars.add_argument("--when", default="onclick", help="Event that will trigger the action")
        pars.add_argument("--tab")

    def effect(self):
        self.ensureInkWebSupport()

        if len(self.options.ids) < 2:
            raise inkex.AbortExtension("You must select at least two elements."
                                       " The last one is the object you want to go to")

        el_from = list(self.svg.selected.values())[:-1]

        ev_code = "InkWeb.moveViewbox({from:this, to:'" + self.options.ids[-1] +"'})"
        for elem in el_from:
            prev_ev_code = elem.get(self.options.when)
            el_ev_code = ev_code +";" + (prev_ev_code or '')
            elem.set(self.options.when, el_ev_code)

if __name__ == '__main__':
    InteractiveMockup().run()
