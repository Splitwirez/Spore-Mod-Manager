#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2006-2019 AUTHORS
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
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110, USA.
#
"""
Open the default operating system web browser at the url specified.
"""
import webbrowser
import threading
from argparse import ArgumentParser

BROWSER = None # Default from 'webbrowser' overright in tests

class ThreadWebsite(threading.Thread):
    """Visit website without locking Inkscape"""
    def __init__(self, args=None):
        threading.Thread.__init__(self)
        parser = ArgumentParser()
        parser.add_argument("-u", "--url",
                            default="https://www.inkscape.org/",
                            help="The URL to open in web browser")
        self.options = parser.parse_args(args)

    def run(self):
        webbrowser.get(BROWSER).open(self.options.url)

if __name__ == '__main__':
    ThreadWebsite().start()
