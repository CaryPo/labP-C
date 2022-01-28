import random
from prettytable import PrettyTable
import time

def shaker_sort(array):

    length = len(array)
    swapped = True
    start = 0
    end = length - 1
    while (swapped == True):
        swapped = False
        # проход слева направо
        for i in range(start, end):
            if i % 2 == 0:
                continue
            else:
                if (array[i] < array[i + 2]):
                    # обмен элементов
                    array[i], array[i + 2] = array[i + 2], array[i]
                    swapped = True

        # если не было обменов прерываем цикл
        if (not (swapped)):
            break
        swapped = False
        end_index = end - 1

        # проход справа налево
        for i in range(end_index - 1, start - 1, -1):
            if i % 2 == 0:
                continue
            else:
                if (array[i] < array[i + 2]):
                    # обмен элементов
                    array[i], array[i + 2] = array[i + 2], array[i]
                    swapped = True

        start = start + 1

def comb(array):
    lengh = len(array)
    swapped = True
    while lengh > 1 or swapped:
        lengh = max(1, int(lengh * 10 / 13))
        swapped = False
        for i in range(len(array) - lengh):

            if array[i] < array[i + lengh]:
                array[i], array[i + lengh] = array[i + lengh], array[i]
                swapped = True
    return array



def main():

    mass = []
    for i in range(int(input("Кол-во элементов в массиве = "))):
        mass.append(random.randint(0, 2000)) #Вводим случайные элементы
    rev_mass = mass
    sort_mass = mass
    rev_mass.reverse()
    sort_mass.sort()


    '''Для того, чтобы выполнить сортировку расческой, нужно разделить массив на четные и нечетные элементы,
    т.к нужно сортировать только нечетные элементы, а шаг между элементами массива в данной сортировке зависит от
    коэффициента сжатия, а значит существкет вероятность, что при проверке целого массива в один момент сравнение может 
    начаться четного и нечетного, что исключено'''
    chet = []
    not_chet = []
    for i in range(len(mass)):
        if i % 2 == 0:
            chet.append(mass[i])
        else:
            not_chet.append(mass[i])
    comb(not_chet)
    result = [None] * (len(chet) + len(not_chet))
    result[::2] = chet  # Объединяем четные и нечетные
    result[1::2] = not_chet

    result_sort = {
        "Название": "",
        "Неотсортированна": "",
        "Отсортированная частично": "",
        "Обратно-отсортированная": ""
    }

    table = PrettyTable()
    table.field_names = (result_sort.keys())

    result_sort["Название"] = "Шейкерная"
    ST = time.time()
    shaker_sort(mass)
    result_sort["Неотсортированна"] = time.time()-ST
    ST = time.time()
    shaker_sort(sort_mass)
    result_sort["Отсортированная частично"] = time.time()-ST
    ST = time.time()
    shaker_sort(rev_mass)
    result_sort["Обратно-отсортированная"] = time.time()-ST
    table.add_row(result_sort.values())

    result_sort["Название"] = "Расческа"
    ST = time.time()
    comb(mass)
    result_sort["Неотсортированна"] = time.time()-ST
    ST = time.time()
    comb(sort_mass)
    result_sort["Отсортированная частично"] = time.time()-ST
    ST = time.time()
    comb(rev_mass)
    result_sort["Обратно-отсортированная"] = time.time()-ST
    table.add_row(result_sort.values())


    print(table)
if __name__ == '__main__':
    main()