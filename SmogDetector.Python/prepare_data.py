# -*- coding: utf-8 -*-


from __future__ import print_function
from __future__ import division
import pandas as pd
import os;

path='E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Python';
os.chdir(path);
myfile='E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Database/tables/NormalizedWeatherData_train.csv';
myfile2='E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Database/tables/ProcessedSmogData.csv';

df=pd.read_csv(myfile, sep=',')
df2=pd.read_csv(myfile2, sep=',')

f1=open('./data/data_reg3_train.txt', 'w+')
last_date='dummy';
tuples =[tuple(x) for x in df2.to_records(index=False)]
tuples_len= len(tuples)
iteration=-1;
last_values={};
for row in df.itertuples():
    if last_date == row[2]:       
        if pd.notnull(row[4]):
            f1.write(" {0}:{1}".format(row[3],row[4]));
            last_values[row[3]]=row[4];
        elif row[3] in last_values:
            f1.write(" {0}:{1}".format(row[3],last_values[row[3]]));
    else:
        last_date = row[2];
        row2 = next(df.iterrows())[1]
        iteration +=1;
        #print("{0} {1}".format(iteration,row[2]))
        if not(row[2] == tuples[min(iteration,tuples_len-1)][1]):
            for new_iter in range(0, len(tuples)):
                if row[2] ==tuples[new_iter][1]:
                    print("Blad Naprawiony {} {} {}".format(min(iteration,tuples_len-1),row[2], tuples[min(iteration,tuples_len-1)][1]));
                    iteration=new_iter;
        if row[2] == tuples[iteration][1]:
            f1.write("\n{} {}:{}".format(tuples[iteration][2],row[3],row[4])); #tuples[iteration][2] dla wartości tuples[iteration][3]dla klasy
        else:
            print("Blad Krytyczny {} {} {}".format(iteration,row[2], tuples[iteration][1]));
f1.close();

    

    
