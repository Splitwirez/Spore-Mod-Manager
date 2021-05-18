# coding=utf-8
import os
from string import Template
import xml.etree.ElementTree as ET
import argparse

parser = argparse.ArgumentParser(description='Reads an *.inx file and generates initialization code for argparse')
parser.add_argument('input')
args = parser.parse_args()

if os.path.isabs(args.input):
    inpath = args.input
else:
    folder = os.path.dirname(os.path.realpath(__file__))
    inpath = os.path.normpath(os.path.join(folder, args.input))

templateWithType = Template('self.arg_parser.add_argument("--$param",  type=$type, dest="$param", default=$default)')
templateWithoutType = Template('self.arg_parser.add_argument("--$param",  dest="$param", default=$default)')
def handle_param_node(node):
    if node.attrib["type"] == 'float':
        cmd = templateWithType.substitute(
            param=node.attrib["name"],
            type='float',
            default=node.text)
        print(cmd)
    elif node.attrib["type"] == 'int':
        cmd = templateWithType.substitute(
            param=node.attrib["name"],
            type='int',
            default=node.text)
        print(cmd)
    elif node.attrib["type"] == 'boolean':
        cmd = templateWithType.substitute(
            param=node.attrib["name"],
            type='inkex.inkbool',
            default='"' + node.text + '"')
        print(cmd)
    elif node.attrib["type"] == 'enum':
        cmd = templateWithoutType.substitute(
            param=node.attrib["name"],
            default='"' + node[0].text + '"')
        print(cmd)
    elif node.attrib["type"] == 'notebook':
        cmd = templateWithoutType.substitute(
            param=node.attrib["name"],
            default='"' + node[0].attrib["name"] + '"')
        print(cmd)
    else:
        # TODO: Implement other types of args
        raise NotImplementedError

def process_node(node):
    for child in node:
        if child.tag.endswith('param'):
            handle_param_node(child)
        process_node(child)

tree = ET.parse(inpath)
root = tree.getroot()
process_node(root)