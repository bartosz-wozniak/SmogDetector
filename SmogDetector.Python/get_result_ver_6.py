# -*- coding: utf-8 -*-
import xgboost as xgb
import matplotlib.pyplot as plt
import os
import sklearn as sk
import common

work_dir= 'E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Python/';
os.chdir(work_dir)
f1=open('./res/res_6.txt', 'w+')
f2=open('./res/res_6_full.txt', 'w+')
# read in data
dtrain = xgb.DMatrix(work_dir+'data/'+'data_class2_train.txt')
dtest = xgb.DMatrix(work_dir+'data/'+'data_class2_test.txt')
# specify parameters via map
param = [
('max_depth',3), # depth of tree
('booster', 'dart'),
#('eta',0.3), #learning rate, prevents overfitting
('silent',1), #  prints mesagees
#('gamma',0.0), #   bigger -> more conservative
#('min_child_weight',1), 
('objective','multi:softmax'),
('num_class',4),
('eval_metric', 'mlogloss'), 
('eval_metric', 'merror') #2 metryki
]



watchlist  = [(dtrain,'train'),(dtest,'eval')]
num_round = 1000
evals_result = {}
for i_max_depth in range(1, 11):
    param[0]=('max_depth',i_max_depth) #zmiana glebokosci drzewa
    print param
    bst = xgb.train(param, dtrain, num_round, watchlist,evals_result=evals_result, early_stopping_rounds=100)
    preds = bst.predict(dtest,ntree_limit=bst.best_ntree_limit)
    print(sk.metrics.classification_report(dtest.get_label(),preds))
    f1.write("{0}\n".format(round(sk.metrics.accuracy_score(dtest.get_label(),preds),2)));
    f2.write('Klasyfikacja DART,'+str(i_max_depth)+','+common.prepare_file_output(dtest.get_label(),preds)+"\n");
f1.close();
f2.close();

#      PLOT 
#ax = xgb.plot_tree(bst,num_trees=0)
#xgb.plot_importance(bst)
#plt.show()
#bst.dump_model('dump_raw.txt');
