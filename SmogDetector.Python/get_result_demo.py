# -*- coding: utf-8 -*-
import xgboost as xgb
import os
import matplotlib.pyplot as plt
os.chdir('E:\krzys\informatyka-studia\sem-16-2017L\msi2\proj\SmogDetector\SmogDetector.Python')
# read in data
dtrain = xgb.DMatrix('E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Python/data/data_reg_train.txt')
dtest = xgb.DMatrix('E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Python/data/data_reg_test.txt')
# specify parameters via map
param = {'max_depth':2, 'eta':1, 'silent':0, 'objective':'reg:logistic' }
num_round = 2
bst = xgb.train(param, dtrain, num_round)
# make prediction
preds = bst.predict(dtest)

ax = xgb.plot_tree(bst,num_trees=0)
xgb.plot_importance(bst)
plt.show()

#todo: przeczytać o parametrach, spróbować innych parametrów :http://xgboost.readthedocs.io/en/latest//parameter.html#
# przeczytać o example: 