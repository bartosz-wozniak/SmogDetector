import os;
work_dir= 'E:/krzys/informatyka-studia/sem-16-2017L/msi2/proj/SmogDetector/SmogDetector.Python/res/';
os.chdir(work_dir)
files=['res_1.txt','res_2.txt','res_3.txt','res_4.txt','res_5.txt','res_6.txt','res_7.txt','res_8.txt','res_9.txt']
#files=['res_1_cv.txt','res_3_cv.txt','res_4_cv.txt','res_5_cv.txt','res_6_cv.txt','res_7_cv.txt']
files_des=[None]* len(files)
for i in range(0,len(files)):
    files_des[i] =open(files[i], 'r');
fout=open('merge.csv', 'w+')
for line in files_des[0]:
    fout.write(line.rstrip())
    for i in range(1,len(files_des)):
        fout.write(','+files_des[i].readline().strip())
    fout.write('\n');
for i in range(0,len(files_des)):
    files_des[i].close();
fout.close()