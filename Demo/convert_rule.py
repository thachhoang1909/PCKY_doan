from nltk.tree import Tree
from nltk.util import breadth_first
from collections import Counter

# convert tree to rule
def convert_rule(tree):
    rule = []
    branch = []
    for b in breadth_first(tree,maxdepth=1):
        branch.append(b)
    g = []
    for b in branch:
        if type(b) == str:
            g.append(b)
        else:
            g.append(b.label())
    rule.append(tuple(g))
    for b in branch[1:]:
        if not type(b)==str:
            rule += convert_rule(b)
    return rule

# calculate probability for rule
def p_rule(rule_list):
    rule_head = Counter()
    rules = Counter()
    rules_with_p = []
    for r in rule_list:
        r = tuple(r)
        rule_head.update({r[0]:1})
        rules.update({r:1})
    for r in rules.keys():
        p = float("{0:.2f}".format(float(rules[r])/float(rule_head[r[0]])))
        if len(r) == 3:
            r = r[0],(r[1],r[2])
        rules_with_p.append((r[0],r[1], p))
    return rules_with_p

# read rule
with open('CNF_rule.txt', 'r') as file:
	rule = []
	for line in file:
		print(line)
		line = line.strip('\n')
		t = Tree.fromstring(line)
		rule += convert_rule(t)

	# add rule with probabilty
	p_rules = p_rule(rule)
	with open('rule.txt', 'w') as file_rule:
		for rules in p_rules:
			for term in rules:
				if type(term) is list or type(term) is tuple:
					for p in term:
						file_rule.write(p)
						file_rule.write(' ')
				else:
					file_rule.write(str(term))
					file_rule.write(' ')
			file_rule.write('\n')




