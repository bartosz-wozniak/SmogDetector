# -*- coding: utf-8 -*-
import xgboost as xgb
import matplotlib.pyplot as plt
import os
import sklearn as sk
import common



work_dir= 'E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Python/';
os.chdir(work_dir)
f1=open('./res/res_9.txt', 'w+')
f2=open('./res/res_9_full.txt', 'w+')
# read in data
dtrain = xgb.DMatrix(work_dir+'data/'+'data_reg2_train.txt')
dtest = xgb.DMatrix(work_dir+'data/'+'data_reg2_test.txt')
# specify parameters via map
param = [
#('max_depth',6), # depth of tree
('booster', 'gblinear'),
#('eta',0.1), #learning rate, prevents overfitting
#('silent',1), #  prints mesagees
#('gamma',1.0), #  bigger -> more conservative
#('min_child_weight',1), 
('objective','reg:linear'),
('eval_metric', 'rmse'), 
#('eval_metric', 'merror') #2 metryki
]

watchlist  = [(dtrain,'train'),(dtest,'eval')]
num_round = 1000
evals_result = {}
#for i_max_depth in range(1, 11):
    #param[0]=('max_depth',i_max_depth) #zmiana glebokosci drzewa
print param
#bst = xgb.train(param, dtrain, num_round, watchlist,evals_result=evals_result, early_stopping_rounds=100)
bst = xgb.train(param, dtrain, num_round, watchlist,feval=common.feval_value,evals_result=evals_result, early_stopping_rounds=100)

#      VALUE TO CLASS
preds = bst.predict(dtest)
preds_class = common.value_to_class_all(preds);
dtest_labels_class =  common.value_to_class_all(dtest.get_label());  
print(sk.metrics.classification_report(dtest_labels_class,preds_class))
f1.write("{0}\n".format(round(sk.metrics.accuracy_score(dtest_labels_class,preds_class),2)));
f2.write('Regresja funkcja linowa,'+str(1)+','+common.prepare_file_output(dtest_labels_class,preds_class)+"\n");
f1.close();
f2.close();

#preds2= bst.predict(dtrain,ntree_limit=bst.best_ntree_limit)
#print(sk.metrics.classification_report(dtest.get_label(),preds_class))
#print sk.metrics.log_loss(dtest.get_label(),preds)


#      PLOT 
#ax = xgb.plot_tree(bst,num_trees=0)
#xgb.plot_importance(bst)
#plt.show()
#bst.dump_model('dump_raw.txt');



