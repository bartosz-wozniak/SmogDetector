# -*- coding: utf-8 -*-
import xgboost as xgb
import matplotlib.pyplot as plt
import os
import sklearn as sk
import common



work_dir= 'E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Python/';
os.chdir(work_dir)
f1=open('./res/res_3_cv.txt', 'w+')
# read in data
dtrain = xgb.DMatrix(work_dir+'data/'+'data_reg2_train.txt')
dtest = xgb.DMatrix(work_dir+'data/'+'data_reg2_test.txt')
# specify parameters via map
param = [
('max_depth',6), # depth of tree
('eta',0.1), #learning rate, prevents overfitting
('silent',1), #  prints mesagees
('gamma',1.0), #  bigger -> more conservative
#('min_child_weight',1), 
('objective','reg:linear'),
('eval_metric', 'rmse'), 
#('eval_metric', 'merror') #2 metryki
]

watchlist  = [(dtrain,'train'),(dtest,'eval')]
num_round = 1000
evals_result = {}
f1.write("Regresja-idx,Regresja-precyzja,Regresja-std\n");
for i_max_depth in range(3,4):
    param[0]=('max_depth',i_max_depth) #zmiana glebokosci drzewa
    print param
    res=xgb.cv(param, dtrain, num_round,nfold=10,stratified=True,feval= common.feval_value, early_stopping_rounds=100)
    print(res)
    min_test = res.loc[:, ['test-error-mean']].min()
    idxmin_test = res.loc[:, ['test-error-mean']].idxmin()
    stdmin_test =  res.loc[idxmin_test,['test-error-std']].iloc[0]
    f1.write("{},{},{}\n".format(int(idxmin_test),round(min_test,2),round(stdmin_test,3)));
f1.close();




