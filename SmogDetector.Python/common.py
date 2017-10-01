#!/usr/bin/python
import sklearn as sk
from numpy import array

def print_heading(class_num):
    concat_str='';
    for i in range(0, class_num):
        concat_str+=str('c'+ str(i)+'_p,'+'c'+ str(i)+'_r,'+'c'+ str(i)+ '_f1,')
    concat_str+='avg/total_p, avg/total_r, avg/total_f1'
    print concat_str;
    return concat_str;
print_heading(4)

def prepare_file_output(dtest_labels_class,preds_class):
    class_stats= sk.metrics.precision_recall_fscore_support(dtest_labels_class,preds_class)
    #accuracy= sk.metrics.accuracy_score(dtest_labels_class,preds_class);
    #print class_stats
    #print accuracy
    concat_str='';
    all_p=0;
    all_r=0;
    all_f1=0;
    all_sup=0;
    for i_class in range(0, len(class_stats[0])):
        #print('class '+ str(i_class)+ ', p: '+str(class_stats[0][i_class])+ ', recall: '+ str(class_stats[1][i_class])+
        #', f1: '+ str(class_stats[2][i_class])+  ', support: '+ str(class_stats[3][i_class]));
        concat_str+=str(str(round(class_stats[0][i_class],2))+ ','+ str(round(class_stats[1][i_class],2))+
        ','+ str(round(class_stats[2][i_class],2))+',');
        all_p+=class_stats[0][i_class]*class_stats[3][i_class]
        all_r+=class_stats[1][i_class]*class_stats[3][i_class]
        all_f1+=class_stats[2][i_class]*class_stats[3][i_class]
        all_sup+=class_stats[3][i_class]
    all_p=all_p/all_sup
    all_r=all_r/all_sup
    all_f1=all_f1/all_sup
    concat_str+=str(str(round(all_p,2))+ ','+ str(round(all_r,2))+
        ','+ str(round(all_f1,2))+',');
    return concat_str


def value_to_class(value):
    ret_val=0;
    if value<33:
        ret_val=0;
    elif value < 50:
        ret_val = 1;
    elif value < 80:
        ret_val = 2;
    else:
        ret_val = 3;
    return ret_val;

def value_small_to_value(value):
    return value*200;
    
def value_small_to_class(value):
    return value_to_class(value_small_to_value(value));

def value_to_class_all(value_list):
    class_list = [None] * len(value_list);
    for i_value in range(0, len(value_list)):
        class_list[i_value] = value_to_class(value_list[i_value]);
    return array(class_list)

def value_small_to_class_all(value_list):
    class_list = [None] * len(value_list);
    for i_value in range(0, len(value_list)):
        class_list[i_value] = value_small_to_class(value_list[i_value]);
    return array(class_list)

def round_all(value_list):
    value_list_r = [None] * len(value_list);
    for i_value in range(0, len(value_list)):
        value_list_r[i_value] = round(value_list[i_value]);
    return array(value_list_r)   


def feval_class(preds_data, dtest_data):
    labels = dtest_data.get_label()
    return 'error', float(sum(labels != preds_data)) / len(labels)

def feval_class_round(preds_data, dtest_data):
    preds_class=round_all(preds_data)
    labels = dtest_data.get_label()
    return 'error', float(sum(labels != preds_class)) / len(labels)


def feval_value_small(preds_data, dtest_data):
    preds_class = value_small_to_class_all(preds_data)
    dtest_labels_class = value_small_to_class_all(dtest_data.get_label())
    return 'error', float(sum(dtest_labels_class != preds_class)) / len(dtest_labels_class)

def feval_value(preds_data, dtest_data):
    preds_class = value_to_class_all(preds_data);
    dtest_labels_class = value_to_class_all(dtest_data.get_label())
    return 'error', float(sum(dtest_labels_class != preds_class)) / len(dtest_labels_class)